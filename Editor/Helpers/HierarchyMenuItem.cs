using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
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
}