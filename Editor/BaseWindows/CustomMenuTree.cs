using System;
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

            EventType currentEventType = currentEvent.type;
            if (currentEventType == EventType.Layout)
                return;

            if (currentEventType == EventType.Repaint || rect.width == 0.0) 
                rect = rect1;

            float y1 = rect.y;
            if (y1 > 1000f)
            {
                float y2 = MenuTree.VisibleRect.y;
                if (y1 + (double) rect.height < y2 ||
                    y1 > y2 + (double) MenuTree.VisibleRect.height)
                {
                    this.MenuItemIsBeingRendered = false;
                    return;
                }
            }

            this.MenuItemIsBeingRendered = true;
            if (currentEventType == EventType.Repaint)
            {
                labelRect = rect;
                labelRect.xMin += 16f + indentLevel * 15f;
                bool isSelected = IsSelected;
                if (isSelected)
                {
                    bool windowInFocus = CustomMenuTree.ActiveMenuTree == MenuTree;
                    Color backgroundColor = windowInFocus
                        ? new Color(0.243f, 0.373f, 0.588f, 1f)
                        : new Color(0.838f, 0.838f, 0.838f, 0.134f);

                    EditorGUI.DrawRect(rect, backgroundColor);
                }
                else
                {
                    if (rect.Contains(currentEvent.mousePosition))
                        EditorGUI.DrawRect(rect, new Color(0.243f, 0.372f, 0.588f, 1f));
                }


                Texture image = IconGetter();
                if (image != null)
                {
                    Rect position = labelRect.AlignLeft(16f).AlignCenter(16f);
                    //position.x += this.Style.IconOffset;
                    if (!isSelected)
                        GUIContentHelper.PushColor(new Color(1f, 1f, 1f, 0.85f));
                    GUI.DrawTexture(position, image, ScaleMode.ScaleToFit);
                    labelRect.xMin += 16f + 3f; // size + padding
                    if (!isSelected)
                        GUIContentHelper.PopColor();
                }

                GUIStyle style = isSelected ? CustomGUIStyles.BoldLabel : CustomGUIStyles.Label;
                var actualLabelRect = labelRect.AlignCenterVertical(16f);
                GUI.Label(actualLabelRect, Name, style);
                if (UseBorders)
                {
                    float num = 1f;
                    if (isSelected)
                        num = 0.0f;

                    Rect rect2 = rect;
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
            EventType type = Event.current.type;
            if (type == EventType.Used && this.wasMouseDownEvent)
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

        public bool HasSelection => !Selection.IsNullOrEmpty();
        public int SelectionCount => Selection?.Count ?? 0;

        public int ToolbarHeight = 22;

        public Rect VisibleRect { get; set; }

        public event Action SelectionChanged; // TODO: how to

        private List<UIMenuItem> _items;

        public CustomMenuTree()
        {
            Selection = new List<UIMenuItem>();
        }

        public IReadOnlyCollection<UIMenuItem> Enumerate() // TODO: how to
        {
            return (IReadOnlyCollection<UIMenuItem>)_items ?? Array.Empty<UIMenuItem>();
        }


        public virtual void Update()
        {
            //OdinMenuTree.HandleKeybaordMenuNavigation();

            if (_items == null)
                return;
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
            if (_items != null)
            {
                foreach (var uiItem in _items)
                {
                    if (uiItem == null)
                        continue;
                    uiItem.DrawMenuItem(evt, 0);
                }
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