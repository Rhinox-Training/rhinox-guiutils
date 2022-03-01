using System;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [IncludeMyAttributes]
    [DontApplyToListElements]
    [HideIf("@$property.ValueEntry.WeakSmartValue == null")]
    public class HideIfNullAttribute : Attribute
    {
        
    }
}