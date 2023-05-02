using System;

namespace Rhinox.GUIUtils.Attributes
{
    public class SmartFallbackDrawnAttribute : Attribute
    {
        public bool AllowUnityIfAble { get; }
        
        public SmartFallbackDrawnAttribute(bool allowUnityIfAble = false)
        {
            AllowUnityIfAble = allowUnityIfAble;
        }
    }
}