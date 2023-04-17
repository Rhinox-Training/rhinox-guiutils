using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseVerticalGroupDrawable<T> : PropertyGroupDrawable<T>
        where T : PropertyGroupAttribute
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
        
        protected BaseVerticalGroupDrawable(GroupedDrawable parent, string groupID, float order) : base(parent, groupID, order)
        {
        }
        
        public override void Draw(GUIContent label)
        {
            if (_drawableMemberChildren == null)
                return;

            GUILayout.BeginVertical(CustomGUIStyles.Clean, GetLayoutOptions(_size));

            for (var i = 0; i < _drawableMemberChildren.Count; i++)
            {
                var childDrawable = _drawableMemberChildren[i];
                if (childDrawable == null || !childDrawable.IsVisible)
                    continue;

                childDrawable.Draw(childDrawable.Label);

                if (_drawableMemberChildren.Count - 1 != i)
                    GUILayout.Space(CustomGUIUtility.Padding); // padding
            }

            GUILayout.EndVertical();
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
    }
}