using System.Linq;
using Rhinox.Lightspeed;
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
            var label = GUIContentHelper.TempContent(Name);
            
            if (IsFoldout())
            {
                GUILayout.Label(label);
                ++EditorGUI.indentLevel;
                
                base.Draw();
                
                --EditorGUI.indentLevel;
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                EditorGUILayout.PrefixLabel(label);
                
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                base.Draw();
                
                EditorGUI.indentLevel = indent;
                GUILayout.EndHorizontal();
            }
        }

        public override void Draw(Rect rect)
        {
            bool isFoldout = IsFoldout();
            var indentLevel = EditorGUI.indentLevel;
            
            if (isFoldout)
            {
                var height = EditorGUIUtility.singleLineHeight;
                var labelRect = rect.AlignTop(height);
                EditorGUI.LabelField(labelRect, GUIContentHelper.TempContent(Name));
                rect.yMin += height;

                ++EditorGUI.indentLevel;
                rect = EditorGUI.IndentedRect(rect);
                EditorGUI.indentLevel = 0;
            }
            else
            {
                rect = EditorGUI.PrefixLabel(rect, GUIContentHelper.TempContent(Name));
            }
            
            base.Draw(rect);

            EditorGUI.indentLevel = indentLevel;
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