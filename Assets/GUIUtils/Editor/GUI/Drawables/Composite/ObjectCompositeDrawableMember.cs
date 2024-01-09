using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
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
                _expanded = eUtility.Foldout(_expanded, label);

                if (_expanded)
                {
                    var indent = EditorGUI.indentLevel;
                    ++EditorGUI.indentLevel;
                
                    foreach (var child in Children)
                        child.Draw(GUIContent.none);
                
                    EditorGUI.indentLevel = indent;
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
            _hasLabel = label != GUIContent.none;

            bool isFoldout = _isFoldout && _hasLabel;

            if (isFoldout)
            {
                var height = EditorGUIUtility.singleLineHeight;
                var labelRect = rect.AlignTop(height);
                _expanded = eUtility.Foldout(labelRect, _expanded, label);

                GUIContentHelper.PushIndentLevel();
                
                if (rect.IsValid())
                    rect.yMin += height + CustomGUIUtility.Padding;

            }
            else if (rect.IsValid())
                rect = EditorGUI.PrefixLabel(rect, label);
            
            if (!isFoldout || _expanded)
            {
                foreach (var child in Children)
                {
                    child.Draw(rect, GUIContent.none);
                } 
            }
            
            if (isFoldout)
                GUIContentHelper.PopIndentLevel();
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