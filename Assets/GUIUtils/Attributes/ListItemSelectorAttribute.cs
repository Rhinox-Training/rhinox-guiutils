using System;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rhinox.GUIUtils.Attributes
{
    [Conditional("UNITY_EDITOR")]
    public class ListItemSelectorAttribute : Attribute
    {
        public string SetSelectedMethod;
    
    #if UNITY_EDITOR
        public Color SelectedColor = EditorGUIUtility.isProSkin ? 
            new Color (91, 91, 91, 255) :
            new Color (222, 222, 222, 255);
    #else
        public Color SelectedColor = Color.white;
    #endif
    
        public ListItemSelectorAttribute(string setSelectedMethod)
        {
            this.SetSelectedMethod = setSelectedMethod;
        }
        
        public ListItemSelectorAttribute(string setSelectedMethod, float r, float g, float b) : this(setSelectedMethod)
        {
            this.SelectedColor = new Color(r, g, b);
        }
    }
}
