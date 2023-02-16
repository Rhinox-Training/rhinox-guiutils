using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
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

            if (currentEventType == EventType.Repaint || rect.width == 0.0) 
                rect = defaultRect;

            float cutoffY = rect.y;
            if (cutoffY > 1000f)
            {
                float visibleY = MenuTree.VisibleRect.y;
                if (cutoffY + (double) rect.height < visibleY ||
                    cutoffY > visibleY + (double) MenuTree.VisibleRect.height)
                {
                    this.MenuItemIsBeingRendered = false;
                    IsHoveringItem = false;
                    return;
                }
            }

            this.MenuItemIsBeingRendered = true;

            if (currentEventType != EventType.Repaint) return;
            
            labelRect = rect;
            labelRect.xMin += 16f + indentLevel * 15f;
            bool isSelected = IsSelected;
            IsHoveringItem = rect.Contains(currentEvent.mousePosition);

            if (isSelected)
            {
                bool windowInFocus = CustomMenuTree.ActiveMenuTree == MenuTree;
                Color backgroundColor = windowInFocus
                    ? new Color(0.243f, 0.373f, 0.588f, 1f)
                    : new Color(0.838f, 0.838f, 0.838f, 0.134f);

                EditorGUI.DrawRect(rect, backgroundColor);
            }
            else if (Selectable && IsHoveringItem)
            {
                EditorGUI.DrawRect(rect, new Color(0.243f, 0.372f, 0.588f, 1f));
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
                Rect borderRect = rect;
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

            return true;
        }

        public void Deselect()
        {
            IsSelected = false;
            //MenuTree.IsDirty = true;
        }
    }

    public class HierarchyMenuItem : UIMenuItem
    {
        public List<UIMenuItem> Children;
        public List<HierarchyMenuItem> SubGroups;

        private Texture _closedIcon;
        private Texture _openIcon;

        public bool Expanded { get; private set; }
        
        public HierarchyMenuItem(CustomMenuTree customMenuTree, string name, bool expanded)
            : base(customMenuTree, name, null)
        {
            _closedIcon = UnityIcon.InternalIcon("d_scrollright@2x");
            _openIcon = UnityIcon.InternalIcon("d_scrolldown@2x");
            SetExpanded(expanded);
            Selectable = false;

            Children = new List<UIMenuItem>();
            SubGroups = new List<HierarchyMenuItem>();
        }

        protected override bool PerformClick()
        {
            SetExpanded(!Expanded);
            return true;
            
        }

        private void SetExpanded(bool value)
        {
            Expanded = value;
            _icon = value ? _openIcon : _closedIcon;
        }
    }

    [Serializable]
    public class CustomMenuTree
    {
        public static CustomMenuTree ActiveMenuTree; // TODO: how to

        public List<UIMenuItem> Selection; // TODO: how to

        public bool HasSelection => !Selection.IsNullOrEmpty();
        public int SelectionCount => Selection?.Count ?? 0;

        public int ToolbarHeight = 22;

        public bool ShowGrouped = true;

        public string GroupingString = "/";
        private bool _groupingIsDirty;

        public Rect VisibleRect { get; set; }
#if ODIN_INSPECTOR
        public OdinMenuStyle DefaultMenuStyle;
        public bool DrawSearchToolbar;
#endif

        public event Action SelectionChanged; // TODO: how to

        private List<UIMenuItem> _items;
        private List<HierarchyMenuItem> _groupingItems;
        private HierarchyMenuItem _rootItems;

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

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    if (item == null)
                        continue;

                    item.Update();
                } 
            }
            
            if (_groupingItems != null && ShowGrouped)
            {
                foreach (var item in _groupingItems)
                {
                    if (item == null)
                        continue;

                    item.Update();
                } 
            }
        }

        public void DrawGroupHeader(HierarchyMenuItem item, Event evt, int indent)
        {
            item.DrawMenuItem(evt, indent);
            if (item.Expanded)
            {
                foreach (var subGroup in item.SubGroups)
                {
                    DrawGroupHeader(subGroup, evt, ++indent);
                }
                
                foreach (var child in item.Children)
                {
                    child.DrawMenuItem(evt, indent+1, (x) => x.Substring(x.LastIndexOf(GroupingString) + GroupingString.Length));
                }
            }
        }

        public virtual void Draw(Event evt)
        {
            VisibleRect = Expand(CustomEditorGUI.GetVisibleRect(), 300f);
            if (_items == null) 
                return;

            if (ShowGrouped)
            {
                if (_groupingItems == null || _groupingIsDirty)
                {
                    CreateGroupingItems();

                    _groupingIsDirty = false;
                }
                
                foreach (var item in _groupingItems)
                    DrawGroupHeader(item, evt, -1);
                
                foreach (var item in _rootItems.Children)
                    item.DrawMenuItem(evt, 0);
            }
            else
            {
                foreach (var uiItem in _items)
                {
                    if (uiItem == null)
                        continue;
                    uiItem.DrawMenuItem(evt, 0);
                }
            }
        }

        private void CreateGroupingItems()
        {
            _groupingItems = new List<HierarchyMenuItem>();
            _rootItems = new HierarchyMenuItem(this, "", true);
            
            var dict = new Dictionary<string, HierarchyMenuItem>();
            foreach (var item in _items)
            {
                var splitI = item.Name.LastIndexOf(GroupingString);
                if (splitI < 0)
                {
                    _rootItems.Children.Add(item);
                    continue;
                }

                var path = item.Name.Substring(0, splitI);
                if (!dict.ContainsKey(path))
                {
                    var parts = path.Split(new[] { GroupingString }, StringSplitOptions.RemoveEmptyEntries);
                    HierarchyMenuItem hierarchy = null;
                    for (int i = 0; i < parts.Length; ++i)
                    {
                        string full = string.Join(GroupingString, parts.Take(i + 1));
                        if (dict.ContainsKey(full))
                        {
                            hierarchy = dict[full];
                            continue;
                        }

                        var next = new HierarchyMenuItem(this, parts[i], false);
                        hierarchy?.SubGroups.Add(next);
                        hierarchy = next;
                        dict[full] = next;
                        _groupingItems.Add(next);
                    }
                }

                dict[path].Children.Add(item);
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
            _groupingIsDirty = true;
        }

        public void AddCustom(UIMenuItem customItem)
        {
            if (customItem == null || customItem.MenuTree != this)
                return;
            if (_items == null)
                _items = new List<UIMenuItem>(); ;
            _items.AddUnique(customItem);
            _groupingIsDirty = true;
        }

        public void SortMenuItemsByName(bool reverseSort = false)
        {
            if (!reverseSort)
                _items.SortBy(x => x.Name);
            else
                _items.SortByDescending(x => x.Name);
        }
    }
}