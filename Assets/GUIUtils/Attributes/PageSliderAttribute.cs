using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

// [assembly:OdinRegisterAttribute(typeof(PageSliderAttribute), "Custom", "Test")]
namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR"), DontApplyToListElements]
    public class PageSliderAttribute : Attribute
    {
        
    }

}
