using System;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public delegate void MenuItemEventHandler(IMenuItem item);
    
    public interface IMenuItem
    {
        string Name { get; }
        string FullPath { get; }
        CustomMenuTree MenuTree { get; }
        
        object RawValue { get; }
        bool IsFunc { get; }
        object GetInstanceValue();
        
        Rect Rect { get; }
        
        event MenuItemEventHandler RightMouseClicked;

        void DrawMenuItem(Event evt, int i, Func<string, string> nameTransformer = null);
        void Update();
        
        void Select(bool addToSelection = false);
        void Deselect();
    }
}