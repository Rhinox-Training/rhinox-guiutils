using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

// [assembly:OdinRegisterAttribute(typeof(PageSliderAttribute), "Custom", "Test")]
namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR"), DontApplyToListElements]
    public class PageSliderAttribute : Attribute
    {
        
    }

}
