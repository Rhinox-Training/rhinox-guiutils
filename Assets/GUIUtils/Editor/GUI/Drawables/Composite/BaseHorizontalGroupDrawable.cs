using System;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseHorizontalGroupDrawable<T> : PropertyGroupDrawable<T>
        where T : PropertyGroupAttribute
    {
        protected readonly SizeResolver _widthResolver;
        protected Rect _cachedRect;
        
        public override float ElementHeight
        {
            get
            {
                if (Children == null)
                    return EditorGUIUtility.singleLineHeight;
                
                return Children.Where(x => x.IsVisible).Max(x => x.ElementHeight);
            }
        }
        
        protected BaseHorizontalGroupDrawable(GroupedDrawable parent, string groupID, int order)
            : base(parent, groupID, order)
        {
            _widthResolver = new SizeResolver();
        }
        
        public override void Draw(GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            float width = _cachedRect.width;
            // if (_size.MinSize > 0)
            //     width = _size.MinSize;
            if (_size.PreferredSize > float.Epsilon)
                width = _size.PreferredSize;
            if (_size.MaxSize > float.Epsilon)
                width = Mathf.Min(_size.MaxSize, width);

            var widths = _widthResolver.Resolve(width, CustomGUIUtility.Padding);

            GUILayout.BeginHorizontal(CustomGUIStyles.Clean, GetLayoutOptions(_size));

            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                GUILayoutOption[] childOptions = widths[i] > 0 ? new [] { GUILayout.Width(widths[i]) } : Array.Empty<GUILayoutOption>();
                // Debug.Log($"\tInner START {widths[i]}");
                GUILayout.BeginVertical(CustomGUIStyles.Clean, childOptions);

                childDrawable.Draw(childDrawable.Label, childOptions);

                GUILayout.EndVertical();
                // Debug.Log($"\tInner END {innerRect.width}");
                
                if (i < _drawableMemberChildren.Count -1) // don't add padding for last item
                    GUILayout.Space(CustomGUIUtility.Padding);
            }

            GUILayout.EndHorizontal();
            // Debug.Log($"{this.Name} - Outer End {rect.width}");
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            if (rect.IsValid())
                _cachedRect = rect;
            
            var widths = _widthResolver.Resolve(_cachedRect.width, CustomGUIUtility.Padding);
            
            Rect childRect = rect;
            for (int i = 0; i < _drawableMemberChildren.Count; ++i)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                if (childRect.IsValid())
                {
                    childRect.width = widths[i];
                    childRect.height = childDrawable.ElementHeight;
                }
                
                childDrawable.Draw(childRect, childDrawable.Label);
                
                if (childRect.IsValid())
                    childRect.x += childRect.width + CustomGUIUtility.Padding;
            }
        }

        private void UpdateWidthManager()
        {
            _widthResolver.Clear();
            _widthResolver.ClearCache();

            foreach (var member in _drawableMemberChildren)
            {
                TryGetSizeInfo(member, out SizeInfo info);
                _widthResolver.Add(info);
            }
        }
        
        private bool TryGetSizeInfo(IOrderedDrawable drawable, out SizeInfo info)
        {
            if (_sizeInfoByDrawable.ContainsKey(drawable))
            {
                // if (drawable is GroupedDrawable groupDrawer && groupDrawer.Children.Count == 1)
                // {
                //     info = _sizeInfoByDrawable.GetOrDefault(groupDrawer.Children.First());
                //     if (info != null)
                //         return true;
                // }
                info = _sizeInfoByDrawable[drawable];
                return true;
            }

            info = SizeInfo.Empty;
            return false;
        }
        
        protected override void OnChildrenChanged()
        {
            base.OnChildrenChanged();        
            UpdateWidthManager();
        }
    }
}