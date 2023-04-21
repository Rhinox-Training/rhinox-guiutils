using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectCompositeDrawableMember : CompositeDrawableMember, IDrawableReadWrite, IHostedDrawable
    {
        public override GUIContent Label => _label;

        private GUIContent _label;
        private bool _hasLabel = false;
        
        public object Instance { get; }
        public Type ObjectType { get; }
        
        private IOrderedDrawable _innerDrawable;
        
        public GenericHostInfo HostInfo { get; }

        public bool IsFoldout => Children.Any(drawable => drawable.IsVisible);

        public override float ElementHeight
        {
            get
            {
                var height = _innerDrawable.ElementHeight;
                if (IsFoldout && _hasLabel)
                    height += EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;
                return height;
            }
        }

        public override bool ShouldRepaint => base.ShouldRepaint || (_innerDrawable != null ? _innerDrawable.ShouldRepaint : false);
        
        public static ObjectCompositeDrawableMember CreateFrom(GenericHostInfo hostInfo, IOrderedDrawable contents, float order = 0)
        {
            var objectCompositeDrawable = new ObjectCompositeDrawableMember(hostInfo, contents, order);
            if (hostInfo != null)
            {
                foreach (var attr in hostInfo.GetAttributes())
                    objectCompositeDrawable.AddAttribute(attr);
            }

            return objectCompositeDrawable;
        }

        private ObjectCompositeDrawableMember(GenericHostInfo hostInfo, IOrderedDrawable contents, float order = 0)
            : this(hostInfo.GetValue(), hostInfo.GetReturnType(), contents, hostInfo?.NiceName ?? "", order)
        {
            Host = hostInfo.Parent ?? hostInfo.GetHost();
            HostInfo = hostInfo;
        }
        
        public ObjectCompositeDrawableMember(object instance, Type type, IOrderedDrawable contents, string name = "", float order = 0)
            : base(name, order)
        {
            Instance = instance;
            ObjectType = type;
            _innerDrawable = contents;
            
            if (name.IsNullOrEmpty())
                _label = GUIContent.none;
            else
                _label = new GUIContent(name);
        }
        
        public override void Draw(GUIContent label)
        {
            _hasLabel = label != GUIContent.none;

            if (IsFoldout)
            {
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
                if (_hasLabel)
                {
                    GUILayout.BeginHorizontal(CustomGUIStyles.Clean);
                    EditorGUILayout.PrefixLabel(label);
                }

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                
                _innerDrawable.Draw(GUIContent.none);
                
                EditorGUI.indentLevel = indent;
                
                if (_hasLabel)
                    GUILayout.EndHorizontal();
            }
        }

        public override void Draw(Rect rect, GUIContent label)
        {
            var indentLevel = EditorGUI.indentLevel;
            
            if (IsFoldout)
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

        public object GetValue()
        {
            return Instance;
        }
        
        public bool TrySetValue(object value)
        {
            if (Host is GenericHostInfo hostInfo)
                return hostInfo.TrySetValue(value);
            if (Host is SerializedProperty prop)
            {
                prop.SetValue(value);
                return true;
            }

            return false;
        }
    }
}