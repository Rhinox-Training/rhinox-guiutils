using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectCompositeDrawableMember : CompositeDrawableMember
    {
        public override float ElementHeight
        {
            get
            {
                var height = base.ElementHeight;
                if (IsFoldout())
                    height += EditorGUIUtility.singleLineHeight;
                return height;
            }
        }

        public ObjectCompositeDrawableMember(string name, float order = 0)
            : base(name, order)
        {
            
        }

        public override void Draw()
        {
            if (!IsFoldout())
                GUILayout.BeginHorizontal();
            
            EditorGUILayout.PrefixLabel(GUIContentHelper.TempContent(Name));
            GUILayout.BeginVertical();
            
            base.Draw();
            
            GUILayout.EndVertical();
            if (!IsFoldout())
                GUILayout.EndHorizontal();
        }

        public override void Draw(Rect rect)
        {
            base.Draw(rect);
        }
        
        private bool IsFoldout()
        {
            int count = 0;
            foreach (var drawable in Children)
            {
                // TODO drawable.IsVisible?
                if (drawable is HideWrapper hiddenDrawable)
                {
                    if (!hiddenDrawable.ShouldDraw())
                        continue;
                }
                ++count;
            }
            return count > 1;
        }
    }
}