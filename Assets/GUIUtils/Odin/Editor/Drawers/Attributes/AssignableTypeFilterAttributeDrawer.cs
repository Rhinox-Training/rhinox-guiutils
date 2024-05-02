using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0.0, 0.0, 2002.0)]
    public sealed class AssignableTypeFilterAttributeDrawer : OdinAttributeDrawer<AssignableTypeFilterAttribute>
    {
        private string _error;
        private GUIContent _label;
        private bool _isList;

        private Func<IEnumerable<object>> _getSelection;
        private IEnumerable<object> _result;
        private Dictionary<object, string> _nameLookup;
        private LocalPersistentContext<bool> _isToggled;
        private GenericSelector<object> _inlineSelector;
        private IEnumerable<object> _nextResult;

        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry != null;
        }

        /// <summary>Initializes this instance.</summary>
        protected override void Initialize()
        {
            _isToggled = this.GetPersistentValue<bool>("Toggled", Attribute.Expanded || SirenixEditorGUI.ExpandFoldoutByDefault);
            _isList = Property.ChildResolver is ICollectionResolver;
            _getSelection = () => Property.ValueEntry.WeakValues.Cast<object>();
            ReloadDropdownCollections();
        }

        private IEnumerable<ValueDropdownItem> GetAllAssignableTypesForTarget()
        {
            var baseType = Attribute.BaseType;
            if (baseType == null)
                baseType = Property.Info.TypeOfValue;

            return ReflectionUtility.GetTypesInheritingFrom(baseType) // TODO already filters Assignable
                    // Only those assignable
                    .Where(baseType.IsAssignableFrom)
                    // Skip any Unity managed types (cannot be assigned)
                    .Where(x => !x.InheritsFrom(typeof(UnityEngine.Object)))
                    .Select(x => new ValueDropdownItem((string) null, x));
        }

        private void ReloadDropdownCollections()
        {
            if (_error != null)
                return;
            IEnumerable<ValueDropdownItem> valueDropdownItems = GetAllAssignableTypesForTarget();
            _nameLookup = new Dictionary<object, string>();
            foreach (ValueDropdownItem valueDropdownItem in valueDropdownItems)
                _nameLookup[(object) valueDropdownItem] = valueDropdownItem.Text;
        }

        /// <summary>
        /// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            _label = label;
            if (Property.ValueEntry == null)
                CallNextDrawer(label);
            else if (_error != null)
            {
                SirenixEditorGUI.ErrorMessageBox(_error, true);
                CallNextDrawer(label);
            }
            else if (_isList)
            {
                CollectionDrawerStaticInfo.NextCustomAddFunction = new Action(OpenSelector);
                CallNextDrawer(label);
                if (_result != null)
                {
                    AddResult(_result);
                    _result = null;
                }

                CollectionDrawerStaticInfo.NextCustomAddFunction = (Action) null;
            }
            else
                DrawDropdown();
        }

        private void AddResult(IEnumerable<object> query)
        {
            if (!query.Any())
                return;
            
            if (_isList)
            {
                ICollectionResolver childResolver = Property.ChildResolver as ICollectionResolver;
                foreach (object obj in query)
                {
                    object[] values = new object[Property.ParentValues.Count];
                    for (int index = 0; index < values.Length; ++index)
                    {
                        if (obj is System.Type type)
                            values[index] = Activator.CreateInstance(type);
                    }

                    childResolver.QueueAdd(values);
                }
            }
            else
            {
                System.Type type = query.FirstOrDefault() as Type;
                for (int index = 0; index < Property.ValueEntry.WeakValues.Count; ++index)
                {
                    if (type != null)
                        Property.ValueEntry.WeakValues[index] = Activator.CreateInstance(type);
                }
            }
        }

        private void DrawDropdown()
        {
            EditorGUI.BeginChangeCheck();
            string currentValueName = GetCurrentValueName();
            IEnumerable<object> query;
            if (Property.Children.Count > 0)
            {
                SirenixEditorGUI.BeginIndentedVertical();
                Rect valueRect;
                _isToggled.Value = SirenixEditorGUI.Foldout(_isToggled.Value, _label, out valueRect);
                query = OdinSelector<object>.DrawSelectorDropdown(valueRect, currentValueName, ShowSelector);
                if (SirenixEditorGUI.BeginFadeGroup(this, _isToggled.Value))
                {
                    ++EditorGUI.indentLevel;
                    for (int index = 0; index < Property.Children.Count; ++index)
                    {
                        InspectorProperty child = Property.Children[index];
                        child.Draw(child.Label);
                    }

                    --EditorGUI.indentLevel;
                }

                SirenixEditorGUI.EndFadeGroup();
                SirenixEditorGUI.EndIndentedVertical();

            }
            else
                query = OdinSelector<object>.DrawSelectorDropdown(_label, currentValueName, ShowSelector, null);

            if (!EditorGUI.EndChangeCheck() || query == null)
                return;
            AddResult(query);
        }

        private void OpenSelector()
        {
            ReloadDropdownCollections();
            ShowSelector(new Rect(Event.current.mousePosition, Vector2.zero)).SelectionConfirmed +=
                (Action<IEnumerable<object>>) (x => _result = x);
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            GenericSelector<object> selector = CreateSelector();
            rect.x = (int) rect.x;
            rect.y = (int) rect.y;
            rect.width = (int) rect.width;
            rect.height = (int) rect.height;
            selector.ShowInPopup(rect, new Vector2(0.0f, 0.0f));
            return selector;
        }

        private GenericSelector<object> CreateSelector()
        {
            IEnumerable<ValueDropdownItem> source1 = GetAllAssignableTypesForTarget() ?? Enumerable.Empty<ValueDropdownItem>();
            bool hasTenOrMoreItems = source1.Take(10).Count() == 10;
            GenericSelector<object> genericSelector = new GenericSelector<object>(Attribute.DropdownTitle, false,
                source1.Select(x => new GenericSelectorItem<object>(x.Text, x.Value)));
            genericSelector.CheckboxToggle = false;
            genericSelector.EnableSingleClickToSelect();
            genericSelector.SelectionTree.Config.DrawSearchToolbar = hasTenOrMoreItems;
            IEnumerable<object> source2 = Enumerable.Empty<object>();
            if (!_isList)
                source2 = _getSelection();
            IEnumerable<object> selection =
                source2.Select(x => x != null ? (object) x.GetType() : (object) null);
            genericSelector.SetSelection(selection);
            genericSelector.SelectionTree.EnumerateTree(false).AddThumbnailIcons(true);
            return genericSelector;
        }

        private string GetCurrentValueName()
        {
            if (EditorGUI.showMixedValue)
                return "—";
            object key = Property.ValueEntry.WeakSmartValue;
            string name = (string) null;
            if (_nameLookup != null && key != null)
                _nameLookup.TryGetValue(key, out name);
            if (key != null)
                key = (object) key.GetType();
            return new GenericSelectorItem<object>(name, key).GetNiceName();
        }
    }
}