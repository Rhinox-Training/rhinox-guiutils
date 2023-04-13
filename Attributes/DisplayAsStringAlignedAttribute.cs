using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)] 
    public class DisplayAsStringAlignedAttribute : PropertyAttribute
    {
        public TextAlignment Alignment { get; }
        
        public bool Overflow { get; }

        public DisplayAsStringAlignedAttribute(bool overflow = true)
        {
            Alignment = TextAlignment.Left;
            Overflow = overflow;
        }

        public DisplayAsStringAlignedAttribute(TextAlignment alignment, bool overflow = true)
        {
            Alignment = alignment;
            Overflow = overflow;
        }
    }
}