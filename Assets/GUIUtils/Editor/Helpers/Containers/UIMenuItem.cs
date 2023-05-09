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
        
        public bool Selectable { get; set; }

        public bool IsHoveringItem { get; private set; }

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

        public void Select(bool multiSelect = false)
        {
            IsSelected = !IsSelected;

            if (MenuTree == null)
                return;
            if (multiSelect && !IsSelected)
                MenuTree.RemoveSelection(this);
            else
            {
                MenuTree.AddSelection(this, !multiSelect);
                IsSelected = true;
            }
        }

        public virtual void Draw(Event currentEvent, int indentLevel, Func<string, string> nameTransformer = null)
        {
            // Claim the needed rect
            Rect defaultRect = GUILayoutUtility.GetRect(0.0f, MenuTree.ItemHeight);

            EventType currentEventType = currentEvent.type;
            
            // The rest is only needed when doing Repaint
            if (currentEventType != EventType.Repaint)
            {
                // must call these events to catch clicks etc
                MenuTree.OnBeginItemDraw(this, Rect, ref defaultRect);
                MenuTree.OnEndItemDraw(this, Rect);
                return;
            }
            
            Rect = defaultRect;

            // Since we don't really know when we're not drawing... Do a rough early cutoff
            float cutoffY = Rect.y;
            if (cutoffY > 1000f)
            {
                float visibleY = MenuTree.VisibleRect.y;
                if (cutoffY + (double) Rect.height < visibleY ||
                    cutoffY > visibleY + (double) MenuTree.VisibleRect.height)
                {
                    IsHoveringItem = false;
                    return;
                }
            }
            
            var labelRect = Rect;

            labelRect.xMin += 16f + indentLevel * 15f;
            IsHoveringItem = Rect.Contains(currentEvent.mousePosition);
            bool windowInFocus = MenuTree.Host == null || MenuTree.Host == EditorWindow.focusedWindow;
            
            if (IsSelected)
            {
                Color backgroundColor = windowInFocus
                    ? CustomGUIStyles.SelectedColor
                    : CustomGUIStyles.UnfocusedSelectedColor;

                EditorGUI.DrawRect(Rect, backgroundColor);
            }
            else if (IsHoveringItem)
            {
                EditorGUI.DrawRect(Rect, CustomGUIStyles.HoverColor);
            }
            
            MenuTree.OnBeginItemDraw(this, Rect, ref labelRect);

            if (_icon != null)
            {
                Rect position = labelRect.AlignLeft(16f).AlignCenter(16f);
                //position.x += this.Style.IconOffset;
                if (!IsSelected)
                    GUIContentHelper.PushColor(Color.white);
                GUI.DrawTexture(position, _icon, ScaleMode.ScaleToFit);
                labelRect.xMin += 16f + 3f; // size + padding
                if (!IsSelected)
                    GUIContentHelper.PopColor();
            }
            

            GUIStyle style = IsSelected ? CustomGUIStyles.BoldLabel : CustomGUIStyles.Label; 
            var actualLabelRect = labelRect.AlignCenterVertical(16f);
            string name = nameTransformer != null ? nameTransformer.Invoke(Name) : Name;
            GUI.Label(actualLabelRect, name, style);
            
            MenuTree.OnEndItemDraw(this, Rect);

            if (UseBorders)
            {
                Rect borderRect = Rect;
                if (!IsSelected)
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

        public bool UseBorders { get; set; } = true;

        public bool IsSelected { get; set; }

        public void SetIcon(Texture icon)
        {
            _icon = icon;
        }

        public virtual void CheckForInteractions()
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
                bool multiSelect = Event.current.modifiers == EventModifiers.Control;
                this.Select(multiSelect);
            }
            else if (Event.current.button == 1)
            {
                RightMouseClicked?.Invoke(this);
            }

            return true;
        }

        public void ResetInteractionState()
        {
            IsHoveringItem = false;
        }

        public void Deselect()
        {
            IsSelected = false;
            //MenuTree.IsDirty = true;
        }
    }
}