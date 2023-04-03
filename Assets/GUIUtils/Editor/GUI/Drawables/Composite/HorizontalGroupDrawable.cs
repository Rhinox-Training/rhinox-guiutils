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
        
        protected readonly SizeResolver _widthResolver;
        private Rect _cachedRect;
        private float _totalCachedWidth;

        public HorizontalGroupDrawable(GroupedDrawable parent, string groupID, int order)
            : base(parent, groupID, order)
        {
            _widthResolver = new SizeResolver();
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

            var widths = _widthResolver.Resolve(width, CustomGUIUtility.Padding);

            // Debug.Log($"{this.Name} - Outer [{_size.MinSize} _{_size.PreferredSize}_ {_size.MaxSize}]");
            var rect = EditorGUILayout.BeginHorizontal(CustomGUIStyles.Clean, GetLayoutOptions(_size));

            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                Rect innerRect = default;
                // Debug.Log($"\tInner {widths[i]}");
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
            
            var widths = _widthResolver.Resolve(_cachedRect.width, CustomGUIUtility.Padding);
            
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
                // No need to trigger ParseAttribute -> should have happened already, but it can't hurt
                ParseAttribute(groupAttribute);
                
                var info = new SizeInfo
                {
                    PreferredSize = groupAttribute.Width,
                    MaxSize = groupAttribute.MaxWidth,
                    MinSize = groupAttribute.MinWidth
                };
                
                EnsureSizeFits(info);
            
                _sizeInfoByDrawable.Add(child, info);
            }
            else // incorrect attribute
                throw new ArgumentException(nameof(attr));
        }

        private readonly List<HorizontalGroupAttribute> _parsedAttributes = new List<HorizontalGroupAttribute>();

        protected override void ParseAttribute(PropertyGroupAttribute attr)
        {
            if (_parsedAttributes.Contains(attr))
                return;
            
            if (attr is HorizontalGroupAttribute groupAttribute)
            {
                _parsedAttributes.Add(groupAttribute);
                SetOrder(groupAttribute.Order);
                
                _size.MinSize = _parsedAttributes.Sum(x => x.Width > 0 ? x.Width : x.MinWidth);
                if (_parsedAttributes.All(x => x.Width > 0))
                    _size.PreferredSize = _parsedAttributes.Sum(x => x.Width);
                else
                    _size.PreferredSize = 0;
                if (_parsedAttributes.All(x => x.Width > 0 || x.MaxWidth > 0))
                    _size.MaxSize = _parsedAttributes.Sum(x => x.Width > 0 ? x.Width : x.MaxWidth);
                else
                    _size.MaxSize = 0;
                
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