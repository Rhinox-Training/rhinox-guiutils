using System;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IMenuItem
    {
        string Name { get; }
        string FullPath { get; }
        object RawValue { get; }
        bool IsFunc { get; }
        CustomMenuTree MenuTree { get; }
        Rect Rect { get; }
        void Update();
        void DrawMenuItem(Event evt, int i, Func<string, string> nameTransformer = null);
        void Deselect();
        object GetInstanceValue();
    }
}