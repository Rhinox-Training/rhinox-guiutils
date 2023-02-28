using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;

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
        public static CustomMenuTree ActiveMenuTree;

        public List<IMenuItem> Selection;

        public bool HasSelection => !Selection.IsNullOrEmpty();
        public int SelectionCount => Selection?.Count ?? 0;

        public int ToolbarHeight = 22;

        public bool ShowGrouped = true;

        public string GroupingString = "/";
        private bool _groupingIsDirty;

        public Rect VisibleRect { get; set; }

        public delegate void SelectionHandler(SelectionChangedType type);
        public event SelectionHandler SelectionChanged; // TODO: how to
        public event Action SelectionConfirmed;

        private List<IMenuItem> _items;
        private List<HierarchyMenuItem> _groupingItems;
        private HierarchyMenuItem _rootItems;

        public IReadOnlyList<IMenuItem> MenuItems => _items.AsReadOnly();
        public object SelectedValue => Selection.IsNullOrEmpty() ? null : Selection[0].RawValue;

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
#endif

        public CustomMenuTree()
        {
            Selection = new List<IMenuItem>();
        }
        
#if ODIN_INSPECTOR
        public CustomMenuTree(OdinMenuTree tree)
        {
            _menuTree = tree;
            Selection = new List<IMenuItem>();
        }
#endif

        public IReadOnlyCollection<UIMenuItem> Enumerate()
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

        public ICollection<UIMenuItem> GetParentMenuItemsRecursive(UIMenuItem item, bool includeSelf = false)
        {
            if (item == null)
                return Array.Empty<UIMenuItem>();
            
            var list = new List<UIMenuItem>();
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

            if (!includeSelf)
                list.Remove(item);
            return list;
        }

        private static void EnumerateSubgroupsAndAddToList(UIMenuItem item, HierarchyMenuItem parentItem, ref List<UIMenuItem> resultSet)
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

        public virtual void Draw()
        {
            var evt = Event.current;
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
            if (Selection == null) return;
            
            Selection.Clear();
            OnSelectionChanged(SelectionChangedType.SelectionCleared);
        }

        private void OnSelectionChanged(SelectionChangedType type)
        {
            SelectionChanged?.Invoke(type);
            // TODO what is confirmed? Should we do it by default when it changes?
            SelectionConfirmed?.Invoke();
        }

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

        public void AddSelection(IMenuItem uiMenuItem, bool clearList)
        {
            if (Selection == null)
                Selection = new List<IMenuItem>();

            if (clearList)
            {
                foreach (var entry in Selection)
                    entry.Deselect();
                Selection.Clear();
            }

            Selection.AddUnique(uiMenuItem);

            OnSelectionChanged(SelectionChangedType.ItemAdded);
        }

        public void Add(string path, object test, Texture icon = null)
        {
            if (_items == null)
                _items = new List<IMenuItem>();

            var item = new UIMenuItem(this, path, test);
            if (icon != null)
                item.SetIcon(icon);
            _items.AddUnique(item);
    #if ODIN_INSPECTOR
            _menuTree.Add(path, test, icon);
    #endif
            
            _groupingIsDirty = true;
        }

        public void AddCustom(IMenuItem customItem)
        {
            if (customItem == null || customItem.MenuTree != this)
                return;
            if (_items == null)
                _items = new List<IMenuItem>(); ;
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