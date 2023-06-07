using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TabGroupDrawable : BaseVerticalGroupDrawable<TabGroupAttribute>
    {
        private Dictionary<string, List<IOrderedDrawable>> _dic;
        private List<string> _tabs;

        public int _index = 0;
        public TabGroupDrawable(GroupedDrawable parent, string groupID, float order) : base(parent, groupID, order)
        {
            _dic = new Dictionary<string, List<IOrderedDrawable>>();
            _tabs = new List<string>();
        }

        protected override void ParseAttributeSmart(IOrderedDrawable child, TabGroupAttribute attr)
        {
            if (!_dic.ContainsKey(attr.TabName))
            { 
                _dic.Add(attr.TabName, new List<IOrderedDrawable>());
                _tabs.Add(attr.TabName);
            }
            
            _dic[attr.TabName].Add(child);
            
            
        }

        protected override void ParseAttributeSmart(TabGroupAttribute attr)
        {
           
            
        }

        public override void Draw(GUIContent label)
        {
            var activeTab = _tabs[_index];
            
            if (_drawableMemberChildren == null)
                return;

            GUILayout.BeginVertical(CustomGUIStyles.Clean, GetLayoutOptions(_size));
            // EditorGUILayout.BeginFadeGroup()
            _index = GUILayout.Toolbar(_index, _tabs.ToArray());
            
            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
      
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;
          
                if (!_dic[activeTab].Contains(childDrawable)) continue;

                childDrawable.Draw(childDrawable.Label);

                if (_drawableMemberChildren.Count - 1 != i)
                    GUILayout.Space(CustomGUIUtility.Padding); // padding
            }

            GUILayout.EndVertical();
        }
    }
}