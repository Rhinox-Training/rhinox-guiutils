using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using UnityEditor;
using UnityEngine;
using Rhinox.Lightspeed;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(ShaderParameterSelectorAttribute))]
    public class ShaderParameterSelectorAttributeDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        
        private PropertyMemberHelper<Shader> _shaderMemberHelper;
        private PropertyMemberHelper<ShaderParameterType> _parameterTypeMemberHelper;

        private string[] _shaderProperties;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute
            ShaderParameterSelectorAttribute attr = attribute as ShaderParameterSelectorAttribute;

            if (!SerializedProperty.DataEquals(_property, property))
            {
                _property = property;
                _shaderMemberHelper = new PropertyMemberHelper<Shader>(property, attr.ShaderProperty);
                _parameterTypeMemberHelper = new PropertyMemberHelper<ShaderParameterType>(property, attr.TypeProperty);
            }
            
            // Errors
            if (!_shaderMemberHelper.ErrorMessage.IsNullOrEmpty())
            {
                EditorGUI.HelpBox(position, _shaderMemberHelper.ErrorMessage, MessageType.Error);
                return;
            }

            if (!_parameterTypeMemberHelper.ErrorMessage.IsNullOrEmpty())
            {
                EditorGUI.HelpBox(position, _parameterTypeMemberHelper.ErrorMessage, MessageType.Error);
                return;
            }
            
            /*if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUILayout.HelpBox("ShaderIDs are session specific and not serializable.", MessageType.Error);
                return;
            }*/
            
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, $"Cannot store a shaderProperty in a {property.propertyType}", MessageType.Error);
                return;
            }

            bool changed = false;
            var shader = _shaderMemberHelper.GetValue(ref changed);
            var shaderType = _parameterTypeMemberHelper.GetValue(ref changed);

            if (changed)
            {
                if (shader == null)
                    _shaderProperties = Array.Empty<string>();
                else
                    _shaderProperties = GetShaderPropertyList(shader, GetValidTypes(shaderType));
            }

            // Actual drawing
            position = EditorGUI.PrefixLabel(position, label);

            if (_shaderProperties.IsNullOrEmpty())
            {
                property.stringValue = EditorGUI.TextField(position, property.stringValue);
                return;
            }

            string dropdownContent = property.stringValue;
            if (!_shaderProperties.Contains(dropdownContent))
            {
                dropdownContent = "";

                const float arrowSize = 20;
                property.stringValue = EditorGUI.TextField(position.PadRight(arrowSize), property.stringValue);
                
                position = position.AlignRight(arrowSize);
            }
            
            if (EditorGUI.DropdownButton(position, new GUIContent(dropdownContent), FocusType.Passive))
            {
                var menu = new GenericMenu();
                foreach (var prop in _shaderProperties)
                    menu.AddItem(
                        new GUIContent(prop),
                        property.stringValue == prop,
                        data =>
                        {
                            property.stringValue = data as string;
                            property.serializedObject.ApplyModifiedProperties();
                        },
                        prop);
                menu.DropDown(position);
            }
        }
        
        private static string[] GetShaderPropertyList(Shader shader, IList<ShaderUtil.ShaderPropertyType> filterTypes = null)
        {
            if (shader == null) return Array.Empty<string>();
            
            List<string> results = new List<string>();

            int count = ShaderUtil.GetPropertyCount(shader);
            results.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                bool isHidden = ShaderUtil.IsShaderPropertyHidden(shader, i);
                var propertyType = ShaderUtil.GetPropertyType(shader, i);
                bool isValidPropertyType = filterTypes == null || filterTypes.Contains(propertyType);
                if (!isHidden && isValidPropertyType)
                {
                    var name = ShaderUtil.GetPropertyName(shader, i);
                    results.Add(name);
                }
            }

            results.Sort();
            return results.ToArray();
        }
        
        private ShaderUtil.ShaderPropertyType[] GetValidTypes(ShaderParameterType type)
        {
            switch (type)
            {
                case ShaderParameterType.Float:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.Float,
                        ShaderUtil.ShaderPropertyType.Range
                    };
                case ShaderParameterType.Color:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.Color,
                    };
                case ShaderParameterType.Vector:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.Vector
                    };
                case ShaderParameterType.Texture:
                    return new[]
                    {
                        ShaderUtil.ShaderPropertyType.TexEnv
                    };
            }

            return null;
        }
    }
}