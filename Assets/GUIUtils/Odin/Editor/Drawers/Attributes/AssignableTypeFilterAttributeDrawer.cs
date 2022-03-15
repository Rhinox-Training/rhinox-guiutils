using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0.0, 0.0, 2002.0)]
    public sealed class AssignableTypeFilterAttributeDrawer : OdinAttributeDrawer<AssignableTypeFilterAttribute>
    {
        private string error;
        private GUIContent label;
        private bool isList;

        private Func<IEnumerable<object>> getSelection;
        private IEnumerable<object> result;
        private Dictionary<object, string> nameLookup;
        private LocalPersistentContext<bool> isToggled;
        private GenericSelector<object> inlineSelector;
        private IEnumerable<object> nextResult;

        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            return property.ValueEntry != null;
        }

        /// <summary>Initializes this instance.</summary>
        protected override void Initialize()
        {
            this.isToggled = this.GetPersistentValue<bool>("Toggled", Attribute.Expanded || SirenixEditorGUI.ExpandFoldoutByDefault);
            this.isList = this.Property.ChildResolver is ICollectionResolver;
            this.getSelection = () => this.Property.ValueEntry.WeakValues.Cast<object>();
            this.ReloadDropdownCollections();
        }

        private IEnumerable<ValueDropdownItem> GetAllAssignableTypesForTarget()
        {
            var baseType = Attribute.BaseType;
            if (baseType == null)
                baseType = Property.Info.TypeOfValue;

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                // Cannot instantiate Abstract and Generic types so skip those
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                // Only those assignable
                .Where(baseType.IsAssignableFrom)
                // Skip any Unity managed types (cannot be assigned)
                .Where(x => !x.InheritsFrom(typeof(Object)))
                .Select(x => new ValueDropdownItem((string) null, x));
        }

        private void ReloadDropdownCollections()
        {
            if (this.error != null)
                return;
            IEnumerable<ValueDropdownItem> valueDropdownItems = GetAllAssignableTypesForTarget();
            this.nameLookup = new Dictionary<object, string>();
            foreach (ValueDropdownItem valueDropdownItem in valueDropdownItems)
                this.nameLookup[(object) valueDropdownItem] = valueDropdownItem.Text;
        }

        /// <summary>
        /// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.label = label;
            if (this.Property.ValueEntry == null)
                this.CallNextDrawer(label);
            else if (this.error != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.error, true);
                CallNextDrawer(label);
            }
            else if (this.isList)
            {
                CollectionDrawerStaticInfo.NextCustomAddFunction = new Action(this.OpenSelector);
                CallNextDrawer(label);
                if (result != null)
                {
                    AddResult(this.result);
                    result = null;
                }

                CollectionDrawerStaticInfo.NextCustomAddFunction = (Action) null;
            }
            else
                this.DrawDropdown();
        }

        private void AddResult(IEnumerable<object> query)
        {
            if (!query.Any())
                return;
            if (this.isList)
            {
                ICollectionResolver childResolver = this.Property.ChildResolver as ICollectionResolver;
                foreach (object obj in query)
                {
                    object[] values = new object[this.Property.ParentValues.Count];
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
                for (int index = 0; index < this.Property.ValueEntry.WeakValues.Count; ++index)
                {
                    if (type != null)
                        this.Property.ValueEntry.WeakValues[index] = Activator.CreateInstance(type);
                }
            }
        }

        private void DrawDropdown()
        {
            EditorGUI.BeginChangeCheck();
            string currentValueName = this.GetCurrentValueName();
            IEnumerable<object> query;
            if (this.Property.Children.Count > 0)
            {
                SirenixEditorGUI.BeginIndentedVertical();
                Rect valueRect;
                this.isToggled.Value = SirenixEditorGUI.Foldout(this.isToggled.Value, this.label, out valueRect);
                query = OdinSelector<object>.DrawSelectorDropdown(valueRect, currentValueName, ShowSelector);
                if (SirenixEditorGUI.BeginFadeGroup(this, this.isToggled.Value))
                {
                    ++EditorGUI.indentLevel;
                    for (int index = 0; index < this.Property.Children.Count; ++index)
                    {
                        InspectorProperty child = this.Property.Children[index];
                        child.Draw(child.Label);
                    }

                    --EditorGUI.indentLevel;
                }

                SirenixEditorGUI.EndFadeGroup();
                SirenixEditorGUI.EndIndentedVertical();

            }
            else
                query = OdinSelector<object>.DrawSelectorDropdown(this.label, currentValueName, this.ShowSelector, null);

            if (!EditorGUI.EndChangeCheck() || query == null)
                return;
            this.AddResult(query);
        }

        private void OpenSelector()
        {
            this.ReloadDropdownCollections();
            this.ShowSelector(new Rect(Event.current.mousePosition, Vector2.zero)).SelectionConfirmed +=
                (Action<IEnumerable<object>>) (x => this.result = x);
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            GenericSelector<object> selector = this.CreateSelector();
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
            GenericSelector<object> genericSelector = new GenericSelector<object>(this.Attribute.DropdownTitle, false,
                source1.Select(x => new GenericSelectorItem<object>(x.Text, x.Value)));
            genericSelector.CheckboxToggle = false;
            genericSelector.EnableSingleClickToSelect();
            genericSelector.SelectionTree.Config.DrawSearchToolbar = hasTenOrMoreItems;
            IEnumerable<object> source2 = Enumerable.Empty<object>();
            if (!this.isList)
                source2 = this.getSelection();
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
            object key = this.Property.ValueEntry.WeakSmartValue;
            string name = (string) null;
            if (this.nameLookup != null && key != null)
                this.nameLookup.TryGetValue(key, out name);
            if (key != null)
                key = (object) key.GetType();
            return new GenericSelectorItem<object>(name, key).GetNiceName();
        }
    }
}