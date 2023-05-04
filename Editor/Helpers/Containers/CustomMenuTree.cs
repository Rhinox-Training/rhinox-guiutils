using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
#if ODIN_INSPECTOR
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;
using CollectionExtensions = Rhinox.Lightspeed.CollectionExtensions;

namespace Rhinox.GUIUtils.Editor
{
    public enum SelectionChangedType
    {
        /// <summary>A menu item was removed.</summary>
        ItemRemoved,

        /// <summary>A menu item was selected.</summary>
        ItemAdded,

        /// <summary>The selection was cleared.</summary>
        SelectionCleared,
    }

    [Serializable]
    public class CustomMenuTree
    {
        public EditorWindow Host;

        public readonly List<IMenuItem> Selection = new List<IMenuItem>();

        public bool HasSelection => Selection.Count > 0;
        public int SelectionCount => Selection.Count;

        public int ToolbarHeight = 22;

        public int ItemHeight = 30;

        public bool ShowGrouped = true;

        public string GroupingString = "/";

        public Rect VisibleRect { get; set; }

        public delegate void SelectionHandler(SelectionChangedType type);

        public event SelectionHandler SelectionChanged; // TODO: how to
        public event Action SelectionConfirmed;

        public delegate void BeginMenuItemHandler(IMenuItem item, Rect fullRect, ref Rect rect);

        public delegate void MenuItemHandler(IMenuItem item, Rect fullRect);

        public event BeginMenuItemHandler BeginItemDraw;
        public event MenuItemHandler EndItemDraw;

        private List<IMenuItem> _items;
#if !ODIN_INSPECTOR
        private List<HierarchyMenuItem> _groupingItems;
        private HierarchyMenuItem _rootItems;
        private bool _groupingIsDirty;
        private string _searchString = string.Empty;
#endif

        public IReadOnlyList<IMenuItem> MenuItems =>
            _items != null ? (IReadOnlyList<IMenuItem>)_items.AsReadOnly() : Array.Empty<IMenuItem>();

        public IReadOnlyCollection<IMenuItem> Enumerate()
        {
            return (IReadOnlyCollection<IMenuItem>)_items ?? Array.Empty<IMenuItem>();
        }

        public object SelectedValue => Selection.Count == 0 ? null : Selection[0].RawValue;

#if ODIN_INSPECTOR
        private OdinMenuTree _menuTree;

        public OdinMenuStyle DefaultMenuStyle
        {
            get => _menuTree?.DefaultMenuStyle;
            set => _menuTree.DefaultMenuStyle = value;
        }
        public bool DrawSearchToolbar
        {
            get => _menuTree?.Config.DrawSearchToolbar ?? false;
            set => _menuTree.Config.DrawSearchToolbar = value;
        }

        public OdinMenuTreeDrawingConfig Config => _menuTree.Config;
        

        public static CustomMenuTree ActiveMenuTree { get; private set; }
#endif

        // =============================================================================================================
        // Constructor

        public CustomMenuTree()
#if ODIN_INSPECTOR
            : this(new OdinMenuTree())
#endif
        {
        }

#if ODIN_INSPECTOR
        public CustomMenuTree(OdinMenuTree tree)
        {
            _menuTree = tree;
            
            _menuTree.Selection.SelectionChanged += OnOdinSelectionChanged;
            _menuTree.Selection.SelectionConfirmed += OnOdinSelectionConfirmed;
        }
#endif

        // =============================================================================================================
        // GUI-methods
        private bool _firstItem;

        public virtual void Draw()
        {
#if ODIN_INSPECTOR
            _menuTree.DrawMenuTree();
#else
            var evt = Event.current;
            VisibleRect = CustomEditorGUI.GetVisibleRect().Padding(-300f);
            if (_items == null)
                return;

            _searchString = GUILayout.TextField(_searchString);
            _firstItem = true;

            if (ShowGrouped)
            {
                if (_groupingItems == null || _groupingIsDirty)
                {
                    CreateGroupingItems();

                    _groupingIsDirty = false;
                }


                foreach (var item in _groupingItems)
                {
                    DrawGroupHeader(item, evt);
                }

                foreach (var item in _rootItems.Children)
                {
                    if(DoesItemMatchSearch(item))
                        DrawItem(item, evt);
                }
            }
            else
            {
                foreach (var uiItem in _items)
                {
                    if (uiItem == null)
                        continue;
                    DrawItem(uiItem, evt);
                }
            }
#endif
        }

        private void DrawItem(IMenuItem item, Event evt, int indent = 0)
        {
            item.UseBorders = !_firstItem;
            _firstItem = false;

            item.Draw(evt, indent);
        }

        public void OnBeginItemDraw(IMenuItem uiItem, Rect fullRect, ref Rect rect)
        {
            BeginItemDraw?.Invoke(uiItem, fullRect, ref rect);
        }

        public void OnEndItemDraw(IMenuItem uiItem, Rect fullRect)
        {
            EndItemDraw?.Invoke(uiItem, fullRect);
        }

#if !ODIN_INSPECTOR
        public void DrawGroupHeader(HierarchyMenuItem item, Event evt, int indent = 0)
        {
            item.UseBorders = !_firstItem;
            _firstItem = false;

            item.Draw(evt, indent);
            if (item.Expanded)
            {
                foreach (var subGroup in item.SubGroups)
                {
                    DrawGroupHeader(subGroup, evt, indent + 1);
                }

                foreach (var child in item.Children)
                {
                    if (DoesItemMatchSearch(child))
                        child.Draw(evt, indent + 1,
                            (x) => x.Substring(x.LastIndexOf(GroupingString) + GroupingString.Length));
                }
            }
        }

        private bool DoesItemMatchSearch(IMenuItem item, Func<IMenuItem, bool> filter = null)
        {
            // Create a local variable to store the FullPath property of the item
            string fullPath = item.FullPath;
            
            // If filter is null, assign a lambda expression to it that always returns true
            if (filter == null)
                filter = i => true;

            // Run the filter and check if the path contains the search string
            return filter(item) && fullPath.Contains(_searchString);
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
                        if (i == 0)
                            _groupingItems.Add(next);
                    }
                }

                dict[path].Children.Add(item);
            }


            foreach (var item in _groupingItems)
            {
                if (item.LinkedMenuItem != null)
                    continue;

                var validItems = _rootItems.Children.Where(x => x.FullPath == item.FullPath).ToArray();
                if (validItems.Length > 0)
                {
                    item.LinkedMenuItem = validItems[0];
                    _rootItems.Children.Remove(validItems[0]);
                }
            }
        }
#endif

        public void HandleRefocus(Rect currentLayoutRect)
        {
#if ODIN_INSPECTOR
            if (CustomMenuTree.ActiveMenuTree == this 
                || Event.current.rawType != UnityEngine.EventType.MouseDown 
                || !currentLayoutRect.Contains(Event.current.mousePosition) 
                || !Sirenix.Utilities.Editor.GUIHelper.CurrentWindowHasFocus)
                return;
            // TODO: handle autofocus of search field not implemented
            //this.regainSearchFieldFocus = true;
            //OdinMenuTree.preventAutoFocus = true;
            CustomMenuTree.ActiveMenuTree = this;
            //UnityEditorEventUtility.EditorApplication_delayCall += (Action) (() => OdinMenuTree.preventAutoFocus = false);
            Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
#endif
        }


        // =============================================================================================================
        // Update

        public virtual void Update()
        {
#if ODIN_INSPECTOR
            _menuTree.HandleKeybaordMenuNavigation();
#endif

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    if (item == null)
                        continue;

                    item.Update();
                }
            }
#if !ODIN_INSPECTOR
            if (_groupingItems != null && ShowGrouped)
            {
                foreach (var item in _groupingItems)
                {
                    if (item == null)
                        continue;

                    item.Update();
                }
            }
#endif
        }

        // =============================================================================================================
        // Data handling

        public void Add(string path, object entryVal, Texture icon = null)
        {
            if (_items == null)
                _items = new List<IMenuItem>();

#if ODIN_INSPECTOR
            var items = _menuTree.AddObjectAtPath(path, entryVal);
            if (icon != null)
                items.AddIcon(icon);
            foreach (var item in items)
            {
                var convertedItem = new OdinWrappedMenuItem(this, item);
                _items.AddUnique(convertedItem);
            }
#else
            var item = new UIMenuItem(this, path, entryVal);
            if (icon != null)
                item.SetIcon(icon);
            _items.AddUnique(item);
            _groupingIsDirty = true;
#endif
        }

        public void AddCustom(IMenuItem customItem)
        {
            if (customItem == null || customItem.MenuTree != this)
                return;
            if (_items == null)
                _items = new List<IMenuItem>();
            ;
            _items.AddUnique(customItem);
#if !ODIN_INSPECTOR
            _groupingIsDirty = true;
#endif
        }


        // =============================================================================================================
        // Selection
        public void AddSelection(IMenuItem uiMenuItem, bool clearList)
        {
            if (clearList)
            {
#if !ODIN_INSPECTOR
                foreach (var entry in Selection)
                    entry.Deselect();
#endif
                Selection.Clear();
            }

            Selection.AddUnique(uiMenuItem);

#if !ODIN_INSPECTOR
            OnSelectionChanged(SelectionChangedType.ItemAdded);
#endif
        }

        public void RemoveSelection(IMenuItem uiMenuItem)
        {
            Selection.Remove(uiMenuItem);
#if !ODIN_INSPECTOR
            OnSelectionChanged(SelectionChangedType.ItemRemoved);
#endif
        }

        public void ClearSelection()
        {
            if (Selection == null) return;

#if ODIN_INSPECTOR
            _menuTree.Selection.Clear();
#endif
            Selection.Clear();
#if !ODIN_INSPECTOR
            OnSelectionChanged(SelectionChangedType.SelectionCleared);
#endif
        }

        // =============================================================================================================
        // Other tree methods

        public void TryExpandAllParentItems(IMenuItem menuItem)
        {
#if ODIN_INSPECTOR
            if (menuItem is OdinWrappedMenuItem odinWrapped)
            {
                odinWrapped.InnerItem.GetParentMenuItemsRecursive(false)
                     .ForEach<OdinMenuItem>((Action<OdinMenuItem>) (x => x.Toggled = true));
            }
#else
            foreach (var entry in GetParentMenuItemsRecursive(menuItem, false))
            {
                if (entry is HierarchyMenuItem hierarchyMenuItem)
                    hierarchyMenuItem.SetExpanded(true);
            }
#endif
        }

#if !ODIN_INSPECTOR
        private ICollection<IMenuItem> GetParentMenuItemsRecursive(IMenuItem item, bool includeSelf = false)
        {
            if (item == null)
                return Array.Empty<IMenuItem>();

            var list = new List<IMenuItem>();
            if (_groupingItems != null)
            {
                foreach (var parentItem in _groupingItems)
                {
                    if (!parentItem.Contains(item))
                        continue;

                    list.Add(parentItem);

                    if (parentItem.Children != null && parentItem.Children.Contains(item))
                    {
                        list.Add(item);
                        break;
                    }

                    EnumerateSubgroupsAndAddToList(item, parentItem, ref list);
                }
            }
            else
            {
                list.Add(item);
            }

            if (!includeSelf)
                list.Remove(item);
            return list;
        }

        private static void EnumerateSubgroupsAndAddToList(IMenuItem item, HierarchyMenuItem parentItem,
            ref List<IMenuItem> resultSet)
        {
            foreach (var childItems in parentItem.SubGroups)
            {
                if (!childItems.Contains(item))
                    continue;

                resultSet.Add(childItems);

                if (childItems.Children != null && childItems.Children.Contains(item))
                {
                    resultSet.Add(item);
                    break;
                }

                EnumerateSubgroupsAndAddToList(item, childItems, ref resultSet);
            }
        }
#endif

        public void SortMenuItemsByName(bool reverseSort = false)
        {
#if ODIN_INSPECTOR
            // TODO: how to handle reverse sorting
            _menuTree.SortMenuItemsByName();
#endif
            if (!reverseSort)
                _items.SortBy(x => x.Name);
            else
                _items.SortByDescending(x => x.Name);
        }

#if ODIN_INSPECTOR
        public void TryUseThumbnailIcons()
        {
            _menuTree.EnumerateTree().AddThumbnailIcons();
        }
#endif

        // =============================================================================================================
        // Event handling

#if ODIN_INSPECTOR
        private void OnOdinSelectionChanged(Sirenix.OdinInspector.Editor.SelectionChangedType obj)
        {
            Selection.Clear();
            Selection.AddRange(_items.OfType<OdinWrappedMenuItem>()
                .Where(x => _menuTree.Selection.Contains(x.InnerItem)).Cast<IMenuItem>());
            int num = (int) obj;
            SelectionChanged?.Invoke((SelectionChangedType)num);
        }

        private void OnOdinSelectionConfirmed(OdinMenuTreeSelection obj)
        {
            SelectionConfirmed?.Invoke();
        }
#else
        private void OnSelectionChanged(SelectionChangedType type)
        {
            SelectionChanged?.Invoke(type);
            // TODO what is confirmed? Should we do it by default when it changes?
            SelectionConfirmed?.Invoke();
        }
#endif

        // =============================================================================================================
        // ODIN-backing helpers
#if ODIN_INSPECTOR
        private class OdinWrappedMenuItem : IMenuItem
        {
            private readonly OdinMenuItem _innerItem;
            public OdinMenuItem InnerItem => _innerItem;

            public OdinWrappedMenuItem(CustomMenuTree customMenuTree, OdinMenuItem item)
            {
                _innerItem = item;
                MenuTree = customMenuTree;
                IsFunc = item.Value is Func<object>;
                item.OnRightClick += HandleRightClick;
            }

            private void HandleRightClick(OdinMenuItem obj)
            {
                if (obj == _innerItem)
                    RightMouseClicked?.Invoke(this);
            }

            public string Name => _innerItem.Name;
            public string FullPath=> _innerItem.GetFullPath();
            public object RawValue => _innerItem.Value;
            public bool IsFunc { get; private set; }
            public CustomMenuTree MenuTree { get; private set;  }
            public Rect Rect => _innerItem.Rect;
            public bool UseBorders { get; set; }

            public event MenuItemEventHandler RightMouseClicked;

            public void Draw(Event evt, int i, Func<string, string> nameTransformer = null)
            {
                _innerItem.DrawMenuItem(i);
            }

            public void Update()
            {
                //_innerItem.
            }

            public void Deselect()
            {
                _innerItem.Deselect();
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
                _innerItem.Select(addToSelection);
                if (MenuTree != null)
                    MenuTree.AddSelection(this, !addToSelection);
            }
        }
#endif
        public IMenuItem GetMenuItem(string menuItemName)
        {
            if (_items == null)
                return null;
            return _items.FirstOrDefault(x => x.Name == null ? menuItemName == null : x.Name.Equals(menuItemName));
        }
    }
}