using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Rhinox.GUIUtils.Attributes
{
    public enum ShaderParameterType
    {
        None,
        Float,
        Color,
        Vector,
        Texture
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")]
    public class ShaderParameterSelectorAttribute : PropertyAttribute
    {
        public string ShaderProperty;

        public string TypeProperty;

        public ShaderParameterSelectorAttribute(string shaderProperty, string typeProperty = null)
        {
            ShaderProperty = shaderProperty;
            TypeProperty = typeProperty;
        }
    }
}