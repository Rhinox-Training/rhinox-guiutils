using System;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class BoolDrawableField : BaseMemberDrawable<bool>
    {
        private bool _left;
        
        public BoolDrawableField(object instance, MemberInfo info) : base(instance, info)
        {
        }

        protected override void Initialize()
        {
            _left = GetDrawableAttribute<ToggleLeftAttribute>() != null;
            base.Initialize();
        }

        protected override bool DrawValue(GUIContent label, bool val, params GUILayoutOption[] options)
        {
            if (_left)
                return EditorGUILayout.ToggleLeft(label, val, CustomGUIStyles.CleanLabelField, options);
            return EditorGUILayout.Toggle(label, val, CustomGUIStyles.CleanLabelField, options);
        }

        protected override bool DrawValue(Rect rect, GUIContent label, bool val)
        {
            if (_left)
                return EditorGUI.ToggleLeft(rect, label, val);
            return EditorGUI.Toggle(rect, label, val);
        }
    }
}