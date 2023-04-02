using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class VerticalGroupDrawable : GroupedDrawable
    {
        public override float ElementHeight
        {
            get
            {
                if (Children == null)
                    return EditorGUIUtility.singleLineHeight;
                
                float height = 0.0f;
                foreach (var child in Children)
                {
                    if (!child.IsVisible)
                        continue;
                    if (height > 0)
                        height += CustomGUIUtility.Padding;
                    height += child.ElementHeight;
                }
                return height;
            }
        }

        public VerticalGroupDrawable()
            : this(null, null, 0)
        {
            
        }
        
        public VerticalGroupDrawable(GroupedDrawable parent, string groupID, float order)
            : base(parent, groupID, order)
        {
        }

        public override void Draw(GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            var rect = EditorGUILayout.BeginVertical(CustomGUIStyles.Clean, GetLayoutOptions(_size));
            
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;
                
                childDrawable.Draw(childDrawable.Label);

                GUILayout.Space(CustomGUIUtility.Padding); // padding
            }
            
            EditorGUILayout.EndVertical();
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            Rect childRect = rect;
            for (int i = 0; i < _drawableMemberChildren.Count; ++i)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;
                
                childRect.width = rect.width;
                childRect.height = childDrawable.ElementHeight;
                childDrawable.Draw(childRect, childDrawable.Label);
                
                childRect.y += childRect.height + CustomGUIUtility.Padding;
            }
        }

        protected override void ParseAttribute(IOrderedDrawable child, PropertyGroupAttribute attr)
        {
            if (attr is VerticalGroupAttribute groupAttribute)
            {
                // TODO

            }
            else // incorrect attribute
                throw new ArgumentException(nameof(attr));            
        }

        protected override void ParseAttribute(PropertyGroupAttribute attr)
        {
            if (attr is VerticalGroupAttribute groupAttribute)
            {
                SetOrder(groupAttribute.Order);
                // TODO
                
                _parent?.EnsureSizeFits(_size);
            }
            else // incorrect attribute
                throw new ArgumentException(nameof(attr));   
        }
    }
}