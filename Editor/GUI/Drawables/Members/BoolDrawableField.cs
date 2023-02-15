using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class BoolDrawableField : BaseDrawable<bool>
    {
        public BoolDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override bool DrawValue(object instance, bool val)
        {
            return EditorGUILayout.Toggle(Label, val);
        }

        protected override bool DrawValue(Rect rect, object instance, bool memberVal)
        {
            return EditorGUI.Toggle(rect, Label, memberVal);
        }
    }
}