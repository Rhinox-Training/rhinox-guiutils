﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IOrderedDrawable
    {
        float Order { get; set; }
        float ElementHeight { get; }
        GenericHostInfo HostInfo { get; }
        bool IsVisible { get; }
        GUIContent Label { get; }
        bool ShouldRepaint { get; }

        IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute;
        void Draw(GUIContent label, params GUILayoutOption[] options);
        void Draw(Rect rect, GUIContent label);
    }
}