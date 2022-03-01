using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class IgnoreInScriptableObjectCreatorAttribute : Attribute {}
}