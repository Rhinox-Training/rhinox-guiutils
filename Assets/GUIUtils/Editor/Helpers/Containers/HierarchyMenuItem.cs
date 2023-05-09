using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HierarchyMenuItem : UIMenuItem
    {
        public List<IMenuItem> Children;
        public List<HierarchyMenuItem> SubGroups;

        /// <summary>
        /// The menu item that is linked to this hierarchy menu item
        /// </summary>
        public IMenuItem LinkedMenuItem;
        
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

            Children = new List<IMenuItem>();
            SubGroups = new List<HierarchyMenuItem>();
        }

        protected override bool PerformClick()
        {
            LinkedMenuItem?.Select();
            
            SetExpanded(!Expanded);
            return true;
            
        }

        public void SetExpanded(bool value)
        {
            Expanded = value;
            _icon = value ? _openIcon : _closedIcon;
        }

        public IEnumerable<IMenuItem> GetAllChildren(bool includeHierarchyItems = false)
        {
            foreach (var item in Children)
                yield return item;
            foreach (var group in SubGroups)
            {
                if (includeHierarchyItems)
                    yield return group;
                
                foreach (var item in group.GetAllChildren(includeHierarchyItems))
                    yield return item;
            }
        }

        public bool Contains(IMenuItem menuItem)
        {
            if (Children != null && Children.Contains(menuItem))
                return true;

            if (SubGroups == null)
                return false;
            foreach (var group in SubGroups)
            {
                if (group == menuItem || group.Contains(menuItem))
                    return true;
            }

            return false;
        }

        public override void CheckForInteractions()
        {
            base.CheckForInteractions();
            if (SubGroups != null)
            {
                foreach (var group in SubGroups)
                    group.CheckForInteractions();
            }

            if (Children != null)
            {
                foreach (var entry in Children)
                    entry.CheckForInteractions();
            }
        }
    }
}