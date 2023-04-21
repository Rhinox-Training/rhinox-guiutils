﻿using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityProperty : BaseMemberDrawable
    {
        public override string LabelString => Property != null ? Property.displayName : base.LabelString;

        public SerializedProperty Property { get; }
        
        public DrawableUnityProperty(SerializedProperty prop)
            : base(prop.GetHostInfo())
        {
            Property = prop;
        }
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options) 
        {
            EditorGUILayout.PropertyField(Property, label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.PropertyField(rect, Property, label);
        }
    }
}