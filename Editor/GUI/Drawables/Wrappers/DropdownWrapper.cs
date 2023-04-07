using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DropdownBaseWrapper : BaseWrapperDrawable
    {
        private IPropertyMemberHelper<IEnumerable> _member;
        
        public override float ElementHeight => EditorGUIUtility.singleLineHeight;
        
        private GUIContent _activeItem;
        private GUIContent _defaultItem = new GUIContent("<None>");

        private Rect _dropdownRect;
        private bool _valueChanged;

        public DropdownBaseWrapper(IOrderedDrawable drawable) : base(drawable)
        {
            _activeItem = _defaultItem;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            OnPreDraw();
            
            _member.DrawError();

            if (_innerDrawable is BaseDrawable inner)
                label = inner.Label;

            EditorGUILayout.BeginHorizontal(options);
            EditorGUILayout.PrefixLabel(label);

            var clicked = EditorGUILayout.DropdownButton(_activeItem, FocusType.Keyboard, options);
            
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.IsValid())
                _dropdownRect = rect;

            if (clicked)
                MakeMenuItems(_dropdownRect);
            
            EditorGUILayout.EndHorizontal();
            
            OnPostDraw();
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            OnPreDraw();
            
            if (_innerDrawable is BaseDrawable inner)
                label = inner.Label;

            rect = EditorGUI.PrefixLabel(rect, label);
            
            if (EditorGUI.DropdownButton(rect, _activeItem, FocusType.Keyboard))
                MakeMenuItems(rect);
            
            _member.DrawError(rect);

            OnPostDraw();
        }

        protected override void OnPostDraw()
        {
            base.OnPostDraw();
            
            if (_valueChanged)
            {
                GUI.changed = true;
                _valueChanged = false;
            }
        }

        private void MakeMenuItems(Rect rect)
        {
            var options = _member.ForceGetValue().Cast<object>().ToArray();
            var menu = new GenericMenu();

            foreach (var item in options)
            {
                if (item is IValueDropdownItem dropdownItem)
                {
                    var text = dropdownItem.GetText();
                    menu.AddItem(text, () =>
                    {
                        SetValue(dropdownItem.GetValue());
                        _activeItem = new GUIContent(text);
                    });
                }
                else
                {
                    
                    var text = item.ToString();
                    menu.AddItem(text, () =>
                    {
                        SetValue(item);
                        _activeItem = new GUIContent(text);
                    });
                }
            }

            if (!options.Any())
                menu.AddDisabledItem(new GUIContent("No items..."), false);
            
            menu.DropDown(rect);
        }

        private MethodInfo _info;
        private void SetValue(object value)
        {
            if (_innerDrawable is IMemberDrawable memberDrawable)
            {
                memberDrawable.Entry.TrySetValue(value);
                return;
            }
            else if (_innerDrawable is IObjectDrawable && _innerDrawable.Host is GenericMemberEntry entry)
            {
                entry.TrySetValue(value);
                return;
            }
            if (_info == null)
            {
                var types = _innerDrawable.GetType().GetArgumentsOfInheritedOpenGenericClass(typeof(BaseMemberDrawable<>));
                var baseClass = typeof(BaseMemberDrawable<>).MakeGenericType(types);
                _info = baseClass.GetMethod("SetSmartValue", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            
            _info.Invoke(_innerDrawable, new[] { value });
            _valueChanged = true;
        }

        [WrapDrawer(typeof(ValueDropdownAttribute), -1)]
        public static BaseWrapperDrawable Create(ValueDropdownAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<IEnumerable>(drawable.Host, attr.MemberName);
            return new DropdownBaseWrapper(drawable)
            {
                _member = member
            };
        }
    }
}