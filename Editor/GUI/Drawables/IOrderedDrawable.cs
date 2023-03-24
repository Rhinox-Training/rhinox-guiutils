using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IOrderedDrawable
    {
        float Order { get; set; }
        float ElementHeight { get; }
        object Host { get; }
        
        ICollection<TAttribute> GetDrawableAttributes<TAttribute>() where TAttribute : Attribute;
        void Draw();
        void Draw(Rect rect);
    }
}