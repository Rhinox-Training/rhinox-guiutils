using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class ShaderParameterSelectorAttributeDrawer : OdinAttributeDrawer<ShaderParameterSelectorAttribute, string>
    {
        private PropertyMemberHelper<Shader> _shaderMemberelper;
        private PropertyMemberHelper<ShaderParameterType> _parameterTypeMemberHelper;

        private List<string> _shaderProperties;

        protected override void Initialize()
        {
            base.Initialize();

            _shaderMemberelper = new PropertyMemberHelper<Shader>(Property, Attribute.ShaderProperty);
            _parameterTypeMemberHelper = new PropertyMemberHelper<ShaderParameterType>(Property, Attribute.TypeProperty);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Errors
            if (!_shaderMemberelper.ErrorMessage.IsNullOrWhitespace())
            {
                SirenixEditorGUI.ErrorMessageBox(_shaderMemberelper.ErrorMessage);
                return;
            }

            if (!_parameterTypeMemberHelper.ErrorMessage.IsNullOrWhitespace())
            {
                SirenixEditorGUI.ErrorMessageBox(_parameterTypeMemberHelper.ErrorMessage);
                return;
            }

            // Fetch Options
            bool changed = false;
            var shader = _shaderMemberelper.GetValue(ref changed);
            var type = _parameterTypeMemberHelper.GetValue(ref changed);
            if (changed)
            {
                _shaderProperties = GetShaderPropertyList(shader, GetValidTypes(type));
            }

            // Draw dropdown
            var content = GUIHelper.TempContent(GetName(ValueEntry.SmartValue));
            var choice = OdinSelector<string>.DrawSelectorDropdown(label, content, CreateSelector);

            if (choice != null && choice.Any())
                ValueEntry.SmartValue = choice.First();
        }

        private OdinSelector<string> CreateSelector(Rect r)
        {
            var selector = new GenericSelector<string>("Shader Properties", _shaderProperties, false, GetName);
            selector.EnableSingleClickToSelect();

            selector.SetSelection(ValueEntry.SmartValue);

            selector.ShowInPopup(r);
            return selector;
        }

        private string GetName(string shaderProperty)
        {
            if (shaderProperty.IsNullOrWhitespace())
                return "<Null>";

            if (_shaderProperties != null && !_shaderProperties.Contains(shaderProperty))
                return $"<Missing> [{shaderProperty}]";

            return shaderProperty;
        }

        private static List<string> GetShaderPropertyList(Shader shader, ShaderUtil.ShaderPropertyType[] filterTypes = null)
        {
            List<string> results = new List<string>();

            if (shader == null) return results;

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
            return results;
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