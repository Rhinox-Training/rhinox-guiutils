using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IEditorDrawable
    {
        float ElementHeight { get; }
        void Draw(GUIContent label, params GUILayoutOption[] options);
        void Draw(Rect rect, GUIContent label);
    }
    
    public interface IOrderedDrawable : IEditorDrawable
    {
        float Order { get; set; }
        GenericHostInfo HostInfo { get; }
        bool IsVisible { get; }
        GUIContent Label { get; }
        
        event Action RepaintRequested;

        IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute;
        void TryInitialize();

    }
}