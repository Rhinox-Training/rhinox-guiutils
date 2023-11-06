using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectCompositeDrawableMember : CompositeDrawableMember, IDrawableReadWrite
    {
        public override GUIContent Label => _label;

        private GUIContent _label;
        private bool _hasLabel = false;
        
        private bool _expanded = true;
        private bool _isFoldout = true;
        
        public override float ElementHeight
        {
            get
            {
                var height = Children.Sum(x => x.ElementHeight);
                if (_isFoldout && _hasLabel)
                {
                    var single = EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;
                    if (_expanded)
                        height += single;
                    else height = single;
                }
                return height;
            }
        }
        
        public static ObjectCompositeDrawableMember CreateFrom(GenericHostInfo hostInfo, VerticalGroupDrawable contents, float order = 0)
        {
            ObjectCompositeDrawableMember objectCompositeDrawable;
            if (contents.Children.Count == 0)
                objectCompositeDrawable = new ObjectCompositeDrawableMember(hostInfo, new UndrawableField(hostInfo), order);
            else
                objectCompositeDrawable = new ObjectCompositeDrawableMember(hostInfo, contents, order);
            
            if (hostInfo != null)
            {
                foreach (var attr in hostInfo.GetAttributes())
                    objectCompositeDrawable.AddAttribute(attr);
            }

            return objectCompositeDrawable;
        }

        private ObjectCompositeDrawableMember(GenericHostInfo hostInfo, IOrderedDrawable contents, float order = 0)
            : base(hostInfo.NiceName, order)
        {
            HostInfo = hostInfo;
            Add(contents);
            
            if (hostInfo.NiceName.IsNullOrEmpty())
                _label = GUIContent.none;
            else
                _label = new GUIContent(hostInfo.NiceName);
        }
        
        public override void Draw(GUIContent label)
        {
            _hasLabel = label != GUIContent.none;

            if (_isFoldout && _hasLabel)
            {
                _expanded = eUtility.FoldoutHeader(_expanded, label, null, CustomGUIStyles.Label);

                if (_expanded)
                {
                    ++EditorGUI.indentLevel;
                
                    foreach (var child in Children)
                        child.Draw(GUIContent.none);
                
                    --EditorGUI.indentLevel;
                }
            }
            else
            {
                if (_hasLabel)
                {
                    GUILayout.BeginHorizontal(CustomGUIStyles.Clean);
                    EditorGUILayout.PrefixLabel(label);
                }

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                foreach (var child in Children)
                    child.Draw(GUIContent.none);
                
                EditorGUI.indentLevel = indent;
                
                if (_hasLabel)
                    GUILayout.EndHorizontal();
            }
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            var indentLevel = EditorGUI.indentLevel;
            _hasLabel = label != GUIContent.none;

            if (_isFoldout && _hasLabel)
            {
                var height = EditorGUIUtility.singleLineHeight + 2;
                var labelRect = rect.AlignTop(height);
                EditorGUI.LabelField(labelRect, label);
                rect.yMin += height;

                ++EditorGUI.indentLevel;
                if (rect.IsValid())
                    rect = EditorGUI.IndentedRect(rect);
                EditorGUI.indentLevel = 0;
            }
            else if (rect.IsValid())
                rect = EditorGUI.PrefixLabel(rect, label);
            
            foreach (var child in Children)
                child.Draw(rect, GUIContent.none);

            EditorGUI.indentLevel = indentLevel;
        }

        public object GetValue()
        {
            return HostInfo.GetValue();
        }
        
        public bool TrySetValue(object value)
        {
            return HostInfo.TrySetValue(value);
        }
    }
}