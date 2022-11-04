using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UIMenuItem
    {
        public string Name { get; }

        public string FullPath => Name; // TODO: how to
        
        public object RawValue { get; }
        
        public bool IsFunc { get; }

        private CustomMenuTree _tree;
        

        public UIMenuItem(string name, object value)
        {
            Name = name;
            RawValue = value;
            IsFunc = value is Func<object>;
        }

        public object GetInstanceValue()
        {
            if (IsFunc)
            {
                var func = RawValue as Func<object>;
                return func?.Invoke();
            }

            return RawValue;
        }

        public void Select(bool addToSelection = false)
        {
            if (_tree != null)
                _tree.AddSelection(this, !addToSelection);
        }
    }
    
    public class CustomMenuTree
    {
        public static CustomMenuTree ActiveMenuTree;  // TODO: how to

        public List<UIMenuItem> Selection;  // TODO: how to

        public bool HasSelection => Selection != null ? Selection.Count != 0 : false;
        public int SelectionCount => Selection != null ? Selection.Count : 0;

        public int ToolbarHeight = 22;

        public event System.Action SelectionChanged;  // TODO: how to

        private List<UIMenuItem> _items;

        public CustomMenuTree()
        {
            Selection = new List<UIMenuItem>();
        }
        
        public IReadOnlyCollection<UIMenuItem> Enumerate()  // TODO: how to
        {
            return (IReadOnlyCollection<UIMenuItem>) _items ?? Array.Empty<UIMenuItem>();
        }
        
        
        public virtual void Update()
        {
            //OdinMenuTree.HandleKeybaordMenuNavigation();
        }

        public virtual void Draw()
        {
            
        }

        public void ClearSelection()
        {
            if (Selection != null)
            {
                Selection.Clear();
                SelectionChanged?.Invoke();
            }
        }

        public void HandleRefocus(Rect currentLayoutRect)
        {
            // TODO
        }

        public void AddSelection(UIMenuItem uiMenuItem, bool clearList)
        {
            if (Selection == null)
                Selection = new List<UIMenuItem>();
            
            if (clearList)
                Selection.Clear();
            Selection.AddUnique(uiMenuItem);
            
            SelectionChanged?.Invoke();
        }

        public void Add(string path, object test, Texture icon = null)
        {
            if (_items == null)
                _items = new List<UIMenuItem>();

            var item = new UIMenuItem(path, test);
            _items.AddUnique(item);
        }
    }
}