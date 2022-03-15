using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class AnimationCurveHeightAttribute : Attribute
    {
        public int height;

        public AnimationCurveHeightAttribute(int height = 20)
        {
            this.height = height;
        }
    }
}