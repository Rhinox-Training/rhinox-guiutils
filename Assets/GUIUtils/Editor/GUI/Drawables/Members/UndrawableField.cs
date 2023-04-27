using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UndrawableField : BaseMemberDrawable
    {
        public UndrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            var actualLabel = FindProperLabel(label);
            EditorGUILayout.LabelField(actualLabel, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var actualLabel = FindProperLabel(label);
            EditorGUI.LabelField(rect, actualLabel);
        }

        protected GUIContent FindProperLabel(GUIContent label)
        {
            if (label == null || label == GUIContent.none)
                return GUIContentHelper.TempContent(_hostInfo.GetReturnType().GetFullNiceName(false));
            return label;
        }
    }
    
    public class UndrawableField<T> : BaseMemberValueDrawable<T>
    {
        public UndrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
            
        }
        protected override T DrawValue(GUIContent label, T value, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(label, options);
            return value;
        }

        protected override T DrawValue(Rect rect, GUIContent label, T value)
        {
            EditorGUI.LabelField(rect, label);
            return value;
        }
    }
}