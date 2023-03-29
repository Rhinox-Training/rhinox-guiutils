using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.NoOdin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(DrawAsReferenceAttribute))]
    internal class DrawAsReferenceDrawer : PropertyDrawer
    {
        private FieldInfo _fi;
        private ICollection<Type> _typeOptions;
        
        private SerializedProperty _property;
        private HostInfo _info;

        private bool _toggled;

        private const float _padding = 2;

        private GUIContent _noneContent = new GUIContent("None");
        private Dictionary<string, GUIContent> _typeContentByName = new Dictionary<string, GUIContent>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.Update(ref _info))
            {
                var type = _info.GetReturnType(false);
                _typeOptions = ReflectionUtility.GetTypesInheritingFrom(type);
            }

            Rect dropdownPosition = position.AlignTop(EditorGUIUtility.singleLineHeight);

            DrawTypePicker(dropdownPosition, property, label);

            EditorGUI.PropertyField(position, property, GUIContent.none, true);

            EditorGUI.EndProperty();
        }

        private void DrawTypePicker(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_info.ArrayIndex >= 0)
            {
                label = GUIContentHelper.TempContent($"{_info.ArrayIndex}: ");
                position = EditorGUI.IndentedRect(position);
                GUI.Label(position, label);
                position = position.AlignRight(position.width - 30);
            }
            else
                position = EditorGUI.PrefixLabel(position, label);

            if (!EditorGUI.DropdownButton(position, GetTypeName(property), FocusType.Passive))
                return;

            var menu = new GenericMenu();
            menu.AddItem(_noneContent, false, SetManagedReference, null);
            foreach (var type in _typeOptions)
                menu.AddItem(
                    new GUIContent(type.Name),
                    false,
                    SetManagedReference,
                    type);
            _property = property;
            menu.DropDown(position);
        }

        private void SetManagedReference(object data)
        {
            var value = (data as Type).CreateInstance();
            _property.managedReferenceValue = value;
            _property.isExpanded = value != null;
            _property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        GUIContent GetTypeName(SerializedProperty property)
        {
            // Cache this string.
            string fullTypeName = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(fullTypeName))
                return _noneContent;

            if (_typeContentByName.TryGetValue(fullTypeName, out var cachedTypeName))
                return cachedTypeName;

            Type type = _info.GetReturnType();

            if (type == null)
                return _noneContent;

            string typeName = type.GetCSharpName();

            GUIContent result = new GUIContent(typeName);
            _typeContentByName[fullTypeName] = result;
            return result;
        }
    }
}