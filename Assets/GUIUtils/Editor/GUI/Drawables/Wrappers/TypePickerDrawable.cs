using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TypePickerDrawable : BaseWrapperDrawable
    {
        private static readonly GUIContent NoneContent = new GUIContent("Null");
        
        private readonly SerializedProperty _serializedProperty;
        private object _managedReferenceValue;
        private SimplePicker<Type> _typePicker;

        public TypePickerDrawable(SerializedProperty property)
            : this(property.GetHostInfo())
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            
            _serializedProperty = property;
        }

        public TypePickerDrawable(GenericHostInfo hostInfo)
            : base(new UndrawableField(hostInfo))
        {
            if (hostInfo == null)
                throw new ArgumentNullException(nameof(hostInfo));
            
            _hostInfo = hostInfo;
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            if (_managedReferenceValue == null)
            {
                Rect dropdownPosition = EditorGUILayout.GetControlRect(true, options).AlignTop(EditorGUIUtility.singleLineHeight);

                if (EditorGUI.DropdownButton(dropdownPosition, NoneContent, FocusType.Keyboard))
                    DoTypeDropdown(dropdownPosition);
            }
            else
            {
                base.DrawInner(label, options);
            }
        }

        protected override void DrawInner(Rect position, GUIContent label)
        {
            if (_managedReferenceValue == null)
            {
                if (EditorGUI.DropdownButton(position, NoneContent, FocusType.Keyboard))
                    DoTypeDropdown(position);
            }
            else
            {
                base.DrawInner(position, label);
            }
        }

        private void DoTypeDropdown(Rect position)
        {
            if (_typePicker != null)
            {
                _typePicker.Show(position);
                return;
            }
            
            var type = _hostInfo.GetReturnType(false);
            var options = ReflectionUtility.GetTypesInheritingFrom(type);

            _typePicker = new TypePicker(options);
            _typePicker.OptionSelected += SetManagedReference;
            _typePicker.Show(position);
        }

        private void SetManagedReference(object data)
        {
            var value = (data as Type).CreateInstance();
            
            _managedReferenceValue = value;
            if (_serializedProperty != null)
            {
                _serializedProperty.managedReferenceValue = value;
                _serializedProperty.isExpanded = value != null;
                _serializedProperty.serializedObject.ApplyModifiedProperties();
            }
            else if (_hostInfo != null)
            {
                _hostInfo.SetValue(value);
            }

            UpdateInnerDrawable();
        }

        private void UpdateInnerDrawable()
        {
            if (_serializedProperty != null)
                _innerDrawable = DrawableFactory.CreateDrawableFor(_serializedProperty);
            else if (_hostInfo != null)
                _innerDrawable = DrawableFactory.CreateDrawableFor(_hostInfo);
        }

    }
}