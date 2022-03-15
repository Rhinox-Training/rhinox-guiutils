using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class InlinePropertyAltAttribute : Attribute
    {
    }
}