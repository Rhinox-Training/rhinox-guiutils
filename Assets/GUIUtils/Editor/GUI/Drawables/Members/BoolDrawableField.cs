using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class BoolDrawableField : BaseMemberDrawable<bool>
    {
        public BoolDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override bool DrawValue(GUIContent label, bool val)
        {
            return EditorGUILayout.Toggle(label, val);
        }

        protected override bool DrawValue(Rect rect, GUIContent label, bool memberVal)
        {
            return EditorGUI.Toggle(rect, label, memberVal);
        }
    }
}