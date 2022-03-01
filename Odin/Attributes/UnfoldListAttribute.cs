using System;
using System.Diagnostics;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Rhinox.GUIUtils.Odin
{
    /// <summary>
    /// Simply displays a list within a box; Has no drag, add, remove, foldout or paging functionality.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
    [Conditional("UNITY_EDITOR")] 
#if ODIN_INSPECTOR
    [DontApplyToListElements]
#endif
    public class UnfoldListAttribute : Attribute
    {
        public TextAlignment LabelAlignment;
        public string OnBeforeTitleGUI;
        public string OnAfterTitleGUI;
        
        public UnfoldListAttribute(TextAlignment labelAlignment = TextAlignment.Right)
        {
            LabelAlignment = labelAlignment;
        }
    }
}