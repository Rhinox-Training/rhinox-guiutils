using System;
using System.Collections;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;
using Rhinox.Lightspeed;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(ShaderParameterSelectorAttribute))]
    public class ShaderParameterSelectorAttributeDrawer : PropertyDrawer
    {
        private SerializedProperty _property;
        
        private SerializedPropertyMemberHelper<Shader> _shaderMemberHelper;
        private SerializedPropertyMemberHelper<ShaderParameterType> _parameterTypeMemberHelper;

        private string[] _shaderProperties;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute
            ShaderParameterSelectorAttribute attr = (ShaderParameterSelectorAttribute) attribute;

            if (!SerializedProperty.DataEquals(_property, property))
            {
                _property = property;
                _shaderMemberHelper = new SerializedPropertyMemberHelper<Shader>(property, attr.ShaderProperty);
                _parameterTypeMemberHelper = new SerializedPropertyMemberHelper<ShaderParameterType>(property, attr.TypeProperty);
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
                    _shaderProperties = ShaderUtility.GetShaderPropertyList(shader, shaderType);
            }

            // Actual drawing
            position = EditorGUI.PrefixLabel(position, label);

            GUIContentHelper.PushIndentLevel(0); // things like TextField will indent again if this is not done
            
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
            
            GUIContentHelper.PopIndentLevel();
        }
        
        
    }
}