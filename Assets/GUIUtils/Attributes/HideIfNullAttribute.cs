using System;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [IncludeMyAttributes]
    [DontApplyToListElements]
    [HideIf("@$property.ValueEntry.WeakSmartValue == null")]
    public class HideIfNullAttribute : Attribute
    {
        
    }
}