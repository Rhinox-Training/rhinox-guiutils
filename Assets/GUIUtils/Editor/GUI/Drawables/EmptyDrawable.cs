using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class EmptyDrawable : IOrderedDrawable
    {
        public float Order { get; set; }
        public float ElementHeight { get; }
        public object Host { get; }
        public bool IsVisible { get; }
        public GUIContent Label { get; }
        public bool ShouldRepaint { get; }
        
        public IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute
        {
            return Array.Empty<TAttribute>();
        }

        public void Draw(GUIContent label, params GUILayoutOption[] options)
        {
            if (label != GUIContent.none)
                EditorGUILayout.LabelField(label, options);
        }

        public void Draw(Rect rect, GUIContent label)
        {
            if (label != GUIContent.none)
                EditorGUI.LabelField(rect, label);
        }
    }
}