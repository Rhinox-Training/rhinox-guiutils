using System;
using System.Diagnostics;
using UnityEngine;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class NavMeshAreaAttribute : PropertyAttribute
    {
        public bool IsMask { get; }
        
        public NavMeshAreaAttribute(bool mask = false)
        {
            IsMask = mask;
        }
    }
}