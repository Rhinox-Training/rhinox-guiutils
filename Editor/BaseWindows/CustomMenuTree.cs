﻿using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UIMenuItem
    {
        public string Name { get; }

        public string FullPath => Name; // TODO: how to

        public object RawValue { get; }

        public bool IsFunc { get; }


        public CustomMenuTree MenuTree { get; private set; }
        private Rect rect;
        private Rect labelRect;


        public bool MenuItemIsBeingRendered { get; private set; }

        public UIMenuItem(CustomMenuTree customMenuTree, string name, object value)
        {
            MenuTree = customMenuTree;
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
            if (MenuTree != null)
                MenuTree.AddSelection(this, !addToSelection);
            IsSelected = true;
        }

        public virtual void DrawMenuItem(Event currentEvent, int indentLevel)
        {
            Rect rect1 = GUILayoutUtility.GetRect(0.0f, 30.0f);

            UnityEngine.EventType currentEventType = currentEvent.type;
            if (currentEventType == UnityEngine.EventType.Layout)
                return;

            if (currentEventType == UnityEngine.EventType.Repaint || (double) this.rect.width == 0.0)
                this.rect = rect1;

            float y1 = this.rect.y;
            if ((double) y1 > 1000.0)
            {
                float y2 = MenuTree.VisibleRect.y;
                if ((double) y1 + (double) this.rect.height < (double) y2 ||
                    (double) y1 > (double) y2 + (double) MenuTree.VisibleRect.height)
                {
                    this.MenuItemIsBeingRendered = false;
                    return;
                }
            }

            this.MenuItemIsBeingRendered = true;
            if (currentEventType == UnityEngine.EventType.Repaint)
            {
                this.labelRect = rect;
                this.labelRect.xMin += 16f + (float) indentLevel * 15f;
                bool isSelected = this.IsSelected;
                if (isSelected)
                {
                    bool windowInFocus = CustomMenuTree.ActiveMenuTree == this.MenuTree;
                    Color backgroundColor = windowInFocus
                        ? new Color(0.243f, 0.373f, 0.588f, 1f)
                        : new Color(0.838f, 0.838f, 0.838f, 0.134f);
                    
                    EditorGUI.DrawRect(this.rect, backgroundColor);
                }
                else
                {
                    if (this.rect.Contains(currentEvent.mousePosition))
                        EditorGUI.DrawRect(this.rect, new Color(0.243f, 0.372f, 0.588f, 1f));
                }


                Texture image = this.IconGetter();
                if ((UnityEngine.Object) image != null)
                {
                    Rect position = this.labelRect.AlignLeft(16f).AlignCenter(16f);
                    //position.x += this.Style.IconOffset;
                    if (!isSelected)
                        GUIContentHelper.PushColor(new Color(1f, 1f, 1f, 0.85f));
                    GUI.DrawTexture(position, image, ScaleMode.ScaleToFit);
                    this.labelRect.xMin += 16f + 3f; // size + padding
                    if (!isSelected)
                        GUIContentHelper.PopColor();
                }

                GUIStyle style = isSelected ? CustomGUIStyles.BoldLabel : CustomGUIStyles.Label;
                var actualLabelRect = this.labelRect.AlignCenterVertical(16f);
                GUI.Label(actualLabelRect, Name, style);
                if (UseBorders)
                {
                    float num = 1f;
                    if (isSelected)
                        num = 0.0f;

                    Rect rect2 = this.rect;
                    rect2.x += num;
                    rect2.width -= num * 2f;
                    
                    
                    CustomEditorGUI.HorizontalLine(rect2, new Color(1f, 1f, 1f, 0.103f));

                    // Color backgroundColor = GUI.backgroundColor;
                    // GUI.backgroundColor = backgroundColor * 0.7058f;
                    // if (Event.current.type == UnityEngine.EventType.Repaint)
                    //     whiteTextureStyle.Draw(rect2, GUIContent.none, false, false, false, false);
                    // GUI.backgroundColor = backgroundColor;
                }
            }
        }

        private Texture IconGetter()
        {
            return _icon;
        }

        private static GUIStyle _whiteTextureStyle2;
        private Texture _icon;
        private bool wasMouseDownEvent;

        internal static GUIStyle whiteTextureStyle
        {
            get
            {
                if (_whiteTextureStyle2 == null)
                {
                    GUIStyle whiteTextureStyle2 = new GUIStyle();
                    whiteTextureStyle2.normal.background = EditorGUIUtility.whiteTexture;
                    _whiteTextureStyle2 = whiteTextureStyle2;
                }

                return _whiteTextureStyle2;
            }
        }

        public const bool UseBorders = true;

        public bool IsSelected { get; set; }

        public void SetIcon(Texture icon)
        {
            _icon = icon;
        }

        public void Update()
        {
            UnityEngine.EventType type = Event.current.type;
            if (type == UnityEngine.EventType.Used && this.wasMouseDownEvent)
                this.wasMouseDownEvent = false;

            if (type != EventType.MouseDown) // Only click on mousedown TODO: ? 
                return;
            
            this.wasMouseDownEvent = false;
            if (!rect.Contains(Event.current.mousePosition))
                return;
            bool isSelected = this.IsSelected;
            if (Event.current.button == 0)
            {
                bool addToSelection = Event.current.modifiers == EventModifiers.Control;
                this.Select(addToSelection);
            }

            CustomEditorGUI.RemoveFocusControl();
            Event.current.Use();
        }

        public void Deselect()
        {
            IsSelected = false;
            //MenuTree.IsDirty = true;
        }
    }

    [Serializable]
    public class CustomMenuTree : ScriptableObject
    {
        public static CustomMenuTree ActiveMenuTree; // TODO: how to

        public List<UIMenuItem> Selection; // TODO: how to

        public bool HasSelection => Selection != null ? Selection.Count != 0 : false;
        public int SelectionCount => Selection != null ? Selection.Count : 0;

        public int ToolbarHeight = 22;

        public Rect VisibleRect { get; set; }

        public event System.Action SelectionChanged; // TODO: how to

        private List<UIMenuItem> _items;

        public CustomMenuTree()
        {
            Selection = new List<UIMenuItem>();
        }

        public IReadOnlyCollection<UIMenuItem> Enumerate() // TODO: how to
        {
            return (IReadOnlyCollection<UIMenuItem>) _items ?? Array.Empty<UIMenuItem>();
        }


        public virtual void Update()
        {
            //OdinMenuTree.HandleKeybaordMenuNavigation();
            foreach (var item in _items)
            {
                if (item == null)
                    continue;
                
                item.Update();
            }
        }

        public virtual void Draw(Event evt)
        {
            VisibleRect = Expand(CustomEditorGUI.GetVisibleRect(), 300f);
            foreach (var uiItem in _items)
            {
                if (uiItem == null)
                    continue;
                uiItem.DrawMenuItem(evt, 0);
            }
        }

        public static Rect Expand(Rect rect, float expand)
        {
            rect.x -= expand;
            rect.y -= expand;
            rect.height += expand * 2f;
            rect.width += expand * 2f;
            return rect;
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
            {
                foreach (var entry in Selection)
                    entry.Deselect();
                Selection.Clear();
            }

            Selection.AddUnique(uiMenuItem);

            SelectionChanged?.Invoke();
        }

        public void Add(string path, object test, Texture icon = null)
        {
            if (_items == null)
                _items = new List<UIMenuItem>();

            var item = new UIMenuItem(this, path, test);
            if (icon != null)
                item.SetIcon(icon);
            _items.AddUnique(item);
        }
    }
}