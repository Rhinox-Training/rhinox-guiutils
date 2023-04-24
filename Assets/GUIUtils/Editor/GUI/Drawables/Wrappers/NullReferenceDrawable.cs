using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class NullReferenceDrawable : BaseWrapperDrawable
    {
        private static readonly GUIContent NoneContent = new GUIContent("<Null>");
        private Dictionary<string, GUIContent> _typeContentByName;
        private List<Type> _typeOptions;
        private readonly SerializedProperty _serializedProperty;
        private readonly GenericHostInfo _hostInfo;
        private object _managedReferenceValue;

        public NullReferenceDrawable(SerializedProperty property) : base(new UndrawableField(property.GetHostInfo()))
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            _serializedProperty = property;
            _hostInfo = property.GetHostInfo();
            _typeContentByName = new Dictionary<string, GUIContent>();
            var type = _hostInfo.GetReturnType(true);
            _typeOptions = ReflectionUtility.GetTypesInheritingFrom(type);
        }

        public NullReferenceDrawable(GenericHostInfo hostInfo) : base(new UndrawableField(hostInfo))
        {
            if (hostInfo == null)
                throw new ArgumentNullException(nameof(hostInfo));
            _serializedProperty = null;
            _hostInfo = hostInfo;
            _typeContentByName = new Dictionary<string, GUIContent>();
            var type = _hostInfo.GetReturnType(true);
            _typeOptions = ReflectionUtility.GetTypesInheritingFrom(type);
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            if (_managedReferenceValue == null)
            {
                Rect dropdownPosition = EditorGUILayout.GetControlRect(true, options).AlignTop(EditorGUIUtility.singleLineHeight);
                DrawTypePicker(dropdownPosition, GUIContent.none);
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
                DrawTypePicker(position, GUIContent.none);
            }
            else
            {
                base.DrawInner(position, label);
            }
        }

        private void DrawTypePicker(Rect position, GUIContent label)
        {
            if (label != null)
                position = EditorGUI.PrefixLabel(position, label);

            if (!EditorGUI.DropdownButton(position, GetTypeName(), FocusType.Passive))
                return;
            
            var menu = new GenericMenu();
            menu.AddItem(NoneContent, false, SetManagedReference, null);
            foreach (var type in _typeOptions)
                menu.AddItem(GUIContentHelper.TempContent(type.Name), false,
                    SetManagedReference,
                    type);
            menu.DropDown(position);
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

        private GUIContent GetTypeName()
        {
            if (_hostInfo != null)
                return GetTypeName(_hostInfo.GetReturnType().FullName);
            if (_serializedProperty != null)
            {
                if (_serializedProperty.propertyType == SerializedPropertyType.ManagedReference)
                    return GetTypeName(_serializedProperty.managedReferenceFullTypename);
                return GetTypeName(_serializedProperty.type);
            }
            return GUIContent.none;
        }

        private GUIContent GetTypeName(string fullTypeName)
        {
            // Cache this string.
            if (string.IsNullOrEmpty(fullTypeName))
                return NoneContent;

            if (_typeContentByName.TryGetValue(fullTypeName, out var cachedTypeName))
                return cachedTypeName;

            Type type = _hostInfo.GetReturnType(true);

            if (type == null)
                return NoneContent;

            string typeName = type.GetCSharpName();

            GUIContent result = new GUIContent(typeName);
            _typeContentByName[fullTypeName] = result;
            return result;
        }

        
    }
}