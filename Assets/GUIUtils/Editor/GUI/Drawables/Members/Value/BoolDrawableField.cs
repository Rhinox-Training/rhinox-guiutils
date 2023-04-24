using System;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class BoolDrawableField : BaseMemberValueDrawable<bool>
    {
        public BoolDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
        }

        protected override bool DrawValue(GUIContent label, bool val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.Toggle(label, val, options);
        }

        protected override bool DrawValue(Rect rect, GUIContent label, bool val)
        {
            return EditorGUI.Toggle(rect, label, val);
        }
    }
}