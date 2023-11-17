using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TabGroupDrawable : BaseVerticalGroupDrawable<TabGroupAttribute>
    {
        private Dictionary<string, List<IOrderedDrawable>> _dic;
        private List<TabGroupName> _tabs;
        public int _index = 0;
        
        private class TabGroupName
        {
            public string TabID;
            public IPropertyMemberHelper<string> TabNameHelper;
        }
        
        public override float ElementHeight
        {
            get
            {
                if (Children == null)
                    return EditorGUIUtility.singleLineHeight;
                
                float height = EditorGUIUtility.singleLineHeight;
                foreach (var child in Children)
                {
                    if (!child.IsVisible)
                        continue;
                    
                    if (_tabs != null && _dic != null && 
                        !_dic[_tabs[_index].TabID].Contains(child)) 
                        continue;
                    
                    if (height > EditorGUIUtility.singleLineHeight)
                        height += CustomGUIUtility.Padding;
                    height += child.ElementHeight;
                }
                return height;
            }
        }
        
        public TabGroupDrawable(GroupedDrawable parent, string groupID, float order) : base(parent, groupID, order)
        {
            _dic = new Dictionary<string, List<IOrderedDrawable>>();
            _tabs = new List<TabGroupName>();
        }

        protected override void ParseAttributeSmart(IOrderedDrawable child, TabGroupAttribute attr)
        {
            if (!_dic.ContainsKey(attr.TabName))
            { 
                _dic.Add(attr.TabName, new List<IOrderedDrawable>());
                var memberHelper = MemberHelper.Create<string>(child.HostInfo, attr.TabName);
                _tabs.Add(new TabGroupName()
                {
                    TabID = attr.TabName,
                    TabNameHelper = memberHelper
                });
            }
            
            _dic[attr.TabName].Add(child);
        }

        protected override void ParseAttributeSmart(TabGroupAttribute attr) { }

        public override void Draw(GUIContent label)
        {
            
            if (_drawableMemberChildren == null)
                return;

            GUILayout.BeginVertical(CustomGUIStyles.Clean, GetLayoutOptions(_size));
            
            _index = GUILayout.Toolbar(_index, _tabs.Select(x => x.TabNameHelper.GetSmartValue()).ToArray());
            
            var activeTab = _tabs[_index];
            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
      
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;
          
                if (!_dic[activeTab.TabID].Contains(childDrawable)) 
                    continue;

                childDrawable.Draw(childDrawable.Label);

                if (i < _drawableMemberChildren.Count - 1)
                    GUILayout.Space(CustomGUIUtility.Padding); // padding while not the last element
            }

            GUILayout.EndVertical();
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            Rect toolbarRect, toolbarContentRect;
            if (rect.IsValid())
                rect.SplitY(EditorGUIUtility.singleLineHeight, out toolbarRect, out toolbarContentRect);
            else
            {
                toolbarRect = rect;
                toolbarContentRect = rect;
            }
            _index = GUI.Toolbar(toolbarRect, _index, _tabs.Select(x => x.TabNameHelper.GetSmartValue()).ToArray());

            var activeTab = _tabs[_index];
            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
      
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;
          
                if (!_dic[activeTab.TabID].Contains(childDrawable)) 
                    continue;

                if (toolbarContentRect.IsValid())
                    toolbarContentRect.height = childDrawable.ElementHeight;

                childDrawable.Draw(toolbarContentRect, childDrawable.Label);

                if (toolbarContentRect.IsValid() && i < _drawableMemberChildren.Count - 1)
                    toolbarContentRect.y += childDrawable.ElementHeight + CustomGUIUtility.Padding;
            }
        }
    }
}