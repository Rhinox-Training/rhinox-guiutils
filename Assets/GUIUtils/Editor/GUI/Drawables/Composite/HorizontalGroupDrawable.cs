using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HorizontalGroupDrawable : GroupedDrawable
    {
        public override float ElementHeight
        {
            get
            {
                if (Children == null)
                    return EditorGUIUtility.singleLineHeight;
                
                return Children.Where(x => x.IsVisible).Max(x => x.ElementHeight);
            }
        }
        
        protected readonly SizeManager _widthManager;
        private Rect _cachedRect;
        private float _totalCachedWidth;

        public HorizontalGroupDrawable(GroupedDrawable parent, string groupID, int order)
            : base(parent, groupID, order)
        {
            _widthManager = new SizeManager();
        }
        

        public override void Draw(GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;
            
            var width = _cachedRect.width;
            if (_size.PreferredSize > float.Epsilon)
                width = _size.PreferredSize;
            if (_size.MaxSize > float.Epsilon)
                width = Mathf.Min(_size.MaxSize, width);

            var widths = _widthManager.Resolve(width, CustomGUIUtility.Padding);

            // Debug.Log($"{this.Name} - Outer {_size}");
            var rect = EditorGUILayout.BeginHorizontal(CustomGUIStyles.Clean, GetLayoutOptions(_size));

            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                Rect innerRect = default;
                // Debug.Log($"\tInner {_widths[i]}");
                GUILayoutOption[] options = new [] { GUILayout.Width(widths[i]) };
                innerRect = EditorGUILayout.BeginVertical(CustomGUIStyles.Clean, options);

                childDrawable.Draw(childDrawable.Label, options);

                EditorGUILayout.EndVertical();
                // Debug.Log($"\tInner End {innerRect.width}");
                
                if (i < _drawableMemberChildren.Count -1) // don't add padding for last item
                    GUILayout.Space(CustomGUIUtility.Padding);
            }

            EditorGUILayout.EndHorizontal();
            // Debug.Log($"{this.Name} - Outer End {rect.width}");

            if (rect.IsValid())
                _cachedRect = rect;
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            if (rect.IsValid())
                _cachedRect = rect;
            
            var widths = _widthManager.Resolve(_cachedRect.width, CustomGUIUtility.Padding);
            
            Rect childRect = rect;
            for (int i = 0; i < _drawableMemberChildren.Count; ++i)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                childRect.width = widths[i];
                childRect.height = childDrawable.ElementHeight;
                childDrawable.Draw(childRect, childDrawable.Label);

                childRect.x += childRect.width + CustomGUIUtility.Padding;
            }
        }

        private void UpdateWidthManager()
        {
            _widthManager.Clear();
            _widthManager.ClearCache();

            foreach (var member in _drawableMemberChildren)
            {
                TryGetSizeInfo(member, out SizeInfo info);
                _widthManager.Add(info);
            }
        }

        private bool TryGetSizeInfo(IOrderedDrawable drawable, out SizeInfo info)
        {
            if (_sizeInfoByDrawable.ContainsKey(drawable))
            {
                info = _sizeInfoByDrawable[drawable];
                return true;
            }

            info = SizeInfo.Empty;
            return false;
        }
        
        protected override void ParseAttribute(IOrderedDrawable child, PropertyGroupAttribute attr)
        {
            if (attr is HorizontalGroupAttribute groupAttribute)
            {
                var info = new SizeInfo
                {
                    PreferredSize = groupAttribute.Width,
                    MaxSize = groupAttribute.MaxWidth,
                    MinSize = groupAttribute.MinWidth
                };
                
                EnsureSizeFits(info);
            
                _sizeInfoByDrawable.Add(child, info);
                
                // No need to trigger ParseAttribute -> should have happened already
                // ParseAttribute(groupAttribute);
            }
            else // incorrect attribute
                throw new ArgumentException(nameof(attr));
        }

        protected override void ParseAttribute(PropertyGroupAttribute attr)
        {
            if (attr is HorizontalGroupAttribute groupAttribute)
            {
                SetOrder(groupAttribute.Order);
                
                _size.MinSize = Mathf.Max(_size.MinSize, groupAttribute.MinWidth);
                _size.PreferredSize = Mathf.Max(_size.PreferredSize, groupAttribute.Width);
                _size.MaxSize = Mathf.Max(_size.MaxSize, groupAttribute.MaxWidth);
                
                _parent?.EnsureSizeFits(_size);
            }
            else // incorrect attribute
                throw new ArgumentException(nameof(attr));
        }

        protected override void OnChildrenChanged()
        {
            base.OnChildrenChanged();        
            UpdateWidthManager();
        }
    }
}