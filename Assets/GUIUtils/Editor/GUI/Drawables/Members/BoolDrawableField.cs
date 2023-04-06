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
        public BoolDrawableField(GenericMemberEntry entry) : base(entry)
        {
        }

        protected override bool DrawValue(GUIContent label, bool val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Toggle(label, val, CustomGUIStyles.CleanLabelField, options);
        }

        protected override bool DrawValue(Rect rect, GUIContent label, bool val)
        {
            return EditorGUI.Toggle(rect, label, val);
        }
    }
}