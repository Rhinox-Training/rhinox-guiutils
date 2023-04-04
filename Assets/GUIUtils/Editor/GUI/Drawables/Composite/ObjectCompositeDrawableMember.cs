using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectCompositeDrawableMember : CompositeDrawableMember
    {
        public override GUIContent Label => _label;

        private GUIContent _label;
        private bool _hasLabel = false;
        
        private IOrderedDrawable _innerDrawable;

        public override float ElementHeight
        {
            get
            {
                var height = _innerDrawable.ElementHeight;
                if (IsFoldout() && _hasLabel)
                    height += EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;
                return height;
            }
        }
        
        
        public ObjectCompositeDrawableMember(GenericMemberEntry entry, IOrderedDrawable contents, float order = 0)
            : this(entry.NiceName, contents, order)
        {
            Host = entry;
        }
        
        public ObjectCompositeDrawableMember(string name, IOrderedDrawable contents, float order = 0)
            : base(name, order)
        {
            _label = new GUIContent(name);
            _innerDrawable = contents;
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
                
                _innerDrawable.Draw(GUIContent.none);
                
                --EditorGUI.indentLevel;
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

                EditorGUILayout.PrefixLabel(label);

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                _innerDrawable.Draw(GUIContent.none);
                
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
            
            _innerDrawable.Draw(rect, GUIContent.none);

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