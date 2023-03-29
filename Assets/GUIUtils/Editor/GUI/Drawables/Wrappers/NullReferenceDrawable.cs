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
        private readonly HostInfo _hostInfo;
        private object _managedReferenceValue;

        public NullReferenceDrawable(SerializedProperty property) : base(new UndrawableField<object>(null, property.FindFieldInfo()))
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            _typeContentByName = new Dictionary<string, GUIContent>();
            _serializedProperty = property;
            _hostInfo = property.GetHostInfo();
            var type = _hostInfo.GetReturnType(true);
            _typeOptions = ReflectionUtility.GetTypesInheritingFrom(type);
        }

        protected override void DrawInner(GUIContent label)
        {
            if (_managedReferenceValue == null)
            {
                Rect dropdownPosition = EditorGUILayout.GetControlRect(true).AlignTop(EditorGUIUtility.singleLineHeight);
                DrawTypePicker(dropdownPosition, _serializedProperty, GUIContent.none);
            }
            else
            {
                base.DrawInner(label);
            }
        }

        protected override void DrawInner(Rect position, GUIContent label)
        {
            if (_managedReferenceValue == null)
            {
                DrawTypePicker(position, _serializedProperty, GUIContent.none);
            }
            else
            {
                base.DrawInner(position, label);
            }
        }

        private void DrawTypePicker(Rect position, SerializedProperty property, GUIContent label)
        {
            if (label != null)
                position = EditorGUI.PrefixLabel(position, label);

            if (!EditorGUI.DropdownButton(position, GetTypeName(property), FocusType.Passive))
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
            _serializedProperty.managedReferenceValue = value;
            _serializedProperty.isExpanded = value != null;
            _serializedProperty.serializedObject.ApplyModifiedProperties();

            UpdateInnerDrawable();
        }

        private void UpdateInnerDrawable()
        {
            var hostInfo = _serializedProperty.GetHostInfo();
            var type = hostInfo.GetReturnType();
            var drawables = DrawableFactory.CreateDrawableMembersFor(_serializedProperty, type);
            var compositeDrawableMember = new CompositeDrawableMember();
            compositeDrawableMember.AddRange(drawables);
            _innerDrawable = compositeDrawableMember;
        }

        GUIContent GetTypeName(SerializedProperty property)
        {
            // Cache this string.
            string fullTypeName = property.managedReferenceFullTypename;

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