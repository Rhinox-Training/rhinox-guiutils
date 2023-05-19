using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TypeDrawableField : BaseMemberValueDrawable<Type>
    {
        private TypePicker _typePicker;
        private Type _newValue;

        private Rect _dropdownRect;
        
        public TypeDrawableField(object instance, MemberInfo info) : base(instance, info)
        {
        }

        public TypeDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
        }
        
        protected GUIContent GetDropdownContent(Type value)
        {
            const string NoneText = "<None>";
            var title = value == null ? NoneText : value.Name;
            return GUIContentHelper.TempContent(title);
        }

        protected override Type DrawValue(GUIContent label, Type value, params GUILayoutOption[] options)
        {
            EditorGUILayout.PrefixLabel(label);
            if (EditorGUILayout.DropdownButton(GetDropdownContent(value), FocusType.Keyboard))
                DoTypeDropdown(_dropdownRect, value);
            
            var rect = GUILayoutUtility.GetLastRect();
            if (rect.IsValid())
                _dropdownRect = rect;

            return GetReturnValue(value);
        }

        protected override Type DrawValue(Rect rect, GUIContent label, Type value)
        {
            var valueRect = EditorGUI.PrefixLabel(rect, label);
            if (EditorGUI.DropdownButton(valueRect, GetDropdownContent(value), FocusType.Keyboard))
                DoTypeDropdown(rect, value);
            
            if (_newValue == null) return value;
            GUI.changed = true;
            return _newValue;
        }

        private Type GetReturnValue(Type value)
        {
            if (_newValue != null)
            {
                value = _newValue;
                GUI.changed = true;
                _newValue = null;
            }
            return value;
        }
        
        private void DoTypeDropdown(Rect position, Type value)
        {
            if (_typePicker == null)
            {
                _typePicker = new TypePicker(typeof(object));
                _typePicker.OptionSelected += SetValue;
            }
            
            _typePicker.Show(position);
        }

        private void SetValue(Type type)
        {
            _newValue = type;
        }
    }
}