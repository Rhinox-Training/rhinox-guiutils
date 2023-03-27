using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectCompositeDrawableMember : CompositeDrawableMember
    {
        private GUIContent _label;

        public override GUIContent Label => _label;

        private bool _hasLabel = false;

        public override float ElementHeight
        {
            get
            {
                var height = base.ElementHeight;
                if (IsFoldout() && _hasLabel)
                    height += EditorGUIUtility.singleLineHeight + 2;
                return height;
            }
        }

        public ObjectCompositeDrawableMember(string name, float order = 0)
            : base(name, order)
        {
            _label = new GUIContent(name);
        }

        public override void Draw(GUIContent label)
        {
            if (IsFoldout())
            {
                _hasLabel = label != GUIContent.none;
                if (_hasLabel)
                {
                    GUILayout.Label(label);
                    GUILayout.Space(2);
                }
                ++EditorGUI.indentLevel;
                
                base.Draw(GUIContent.none);
                
                --EditorGUI.indentLevel;
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                EditorGUILayout.PrefixLabel(label);

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                base.Draw(GUIContent.none);
                
                EditorGUI.indentLevel = indent;
                GUILayout.EndHorizontal();
            }
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            bool isFoldout = IsFoldout();
            var indentLevel = EditorGUI.indentLevel;
            
            if (isFoldout)
            {
                _hasLabel = label != GUIContent.none;
                if (_hasLabel)
                {
                    var height = EditorGUIUtility.singleLineHeight + 2;
                    var labelRect = rect.AlignTop(height);
                    EditorGUI.LabelField(labelRect, label);
                    rect.yMin += height;
                }

                ++EditorGUI.indentLevel;
                rect = EditorGUI.IndentedRect(rect);
                EditorGUI.indentLevel = 0;
            }
            else
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            
            base.Draw(rect, GUIContent.none);

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