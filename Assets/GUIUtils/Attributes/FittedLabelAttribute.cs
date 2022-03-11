using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Odin
{
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class FittedLabelAttribute : Attribute
    {
        /// <summary>The new text of the label.</summary>
        public string Text;
        
        public FittedLabelAttribute() {}

        /// <summary>Give a property a custom label. The label will be resized to just fit the given text.</summary>
        /// <param name="text">The new text of the label.</param>
        public FittedLabelAttribute(string text)
        {
            Text = text;
        }
    }
}