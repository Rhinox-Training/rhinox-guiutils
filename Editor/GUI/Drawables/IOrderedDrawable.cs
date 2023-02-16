using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IOrderedDrawable
    {
        float Order { get; set; }
        float ElementHeight { get; }
        ICollection<TAttribute> GetMemberAttributes<TAttribute>() where TAttribute : Attribute;
        void Draw();
        void Draw(Rect rect);
    }
}