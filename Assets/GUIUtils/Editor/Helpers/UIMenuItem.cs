using System;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UIMenuItem : IMenuItem
    {
        public string Name { get; }

        public string FullPath => Name; // TODO: how to

        public object RawValue { get; }

        public bool IsFunc { get; }

        public CustomMenuTree MenuTree { get; private set; }
        
        public Rect Rect { get; private set; }
        public event MenuItemEventHandler RightMouseClicked;
        
        private Rect labelRect;

        public bool Selectable { get; set; }

        public bool IsHoveringItem { get; private set; }
        public bool MenuItemIsBeingRendered { get; private set; }

        public UIMenuItem(CustomMenuTree customMenuTree, string name, object value)
        {
            MenuTree = customMenuTree;
            Name = name;
            RawValue = value;
            IsFunc = value is Func<object>;
            Selectable = true;
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

        public virtual void DrawMenuItem(Event currentEvent, int indentLevel, Func<string, string> nameTransformer = null)
        {
            Rect defaultRect = GUILayoutUtility.GetRect(0.0f, 30.0f);

            EventType currentEventType = currentEvent.type;
            if (currentEventType == EventType.Layout)
                return;

            if (currentEventType == EventType.Repaint || Rect.width == 0.0) 
                Rect = defaultRect;

            float cutoffY = Rect.y;
            if (cutoffY > 1000f)
            {
                float visibleY = MenuTree.VisibleRect.y;
                if (cutoffY + (double) Rect.height < visibleY ||
                    cutoffY > visibleY + (double) MenuTree.VisibleRect.height)
                {
                    this.MenuItemIsBeingRendered = false;
                    IsHoveringItem = false;
                    return;
                }
            }

            this.MenuItemIsBeingRendered = true;

            if (currentEventType != EventType.Repaint) return;
            
            labelRect = Rect;
            labelRect.xMin += 16f + indentLevel * 15f;
            bool isSelected = IsSelected;
            IsHoveringItem = Rect.Contains(currentEvent.mousePosition);

            if (isSelected)
            {
                bool windowInFocus = CustomMenuTree.ActiveMenuTree == MenuTree;
                Color backgroundColor = windowInFocus
                    ? new Color(0.243f, 0.373f, 0.588f, 1f)
                    : new Color(0.838f, 0.838f, 0.838f, 0.134f);

                EditorGUI.DrawRect(Rect, backgroundColor);
            }
            else if (Selectable && IsHoveringItem)
            {
                EditorGUI.DrawRect(Rect, new Color(0.243f, 0.372f, 0.588f, 1f));
            }

            if (_icon != null)
            {
                Rect position = labelRect.AlignLeft(16f).AlignCenter(16f);
                //position.x += this.Style.IconOffset;
                if (!isSelected)
                    GUIContentHelper.PushColor(new Color(1f, 1f, 1f, 0.85f));
                GUI.DrawTexture(position, _icon, ScaleMode.ScaleToFit);
                labelRect.xMin += 16f + 3f; // size + padding
                if (!isSelected)
                    GUIContentHelper.PopColor();
            }

            GUIStyle style = isSelected ? CustomGUIStyles.BoldLabel : CustomGUIStyles.Label;
            var actualLabelRect = labelRect.AlignCenterVertical(16f);
            string name = nameTransformer != null ? nameTransformer.Invoke(Name) : Name;
            GUI.Label(actualLabelRect, name, style);
            if (UseBorders)
            {
                Rect borderRect = Rect;
                if (!isSelected)
                {
                    borderRect.x += 1.0f;
                    borderRect.width -= 2.0f;
                }
                    
                CustomEditorGUI.HorizontalLine(borderRect, new Color(1f, 1f, 1f, 0.103f));
            }
        }

        private static GUIStyle _whiteTextureStyle2;
        protected Texture _icon;

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
            EventType type = Event.current.type;

            if (IsHoveringItem && type == EventType.MouseDown)
            {
                if (PerformClick())
                {
                    CustomEditorGUI.RemoveFocusControl();
                    Event.current.Use();
                }
            }
        }

        protected virtual bool PerformClick()
        {
            if (!Selectable)
                return false;
            
            if (Event.current.button == 0)
            {
                bool addToSelection = Event.current.modifiers == EventModifiers.Control;
                this.Select(addToSelection);
            }
            else if (Event.current.button == 1)
            {
                RightMouseClicked?.Invoke(this);
            }

            return true;
        }

        public void Deselect()
        {
            IsSelected = false;
            //MenuTree.IsDirty = true;
        }
    }
}