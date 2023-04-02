using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class EnumDrawableField: BaseMemberDrawable<Enum>
    {
        public bool HasFlags { get; }
        
        public EnumDrawableField(object instance, MemberInfo info) : base(instance, info)
        {
            HasFlags = info.GetReturnType().GetCustomAttribute<FlagsAttribute>() != null;
        }

        protected override Enum DrawValue(GUIContent label, Enum memberVal, params GUILayoutOption[] options)
        {
            if (HasFlags)
                return EditorGUILayout.EnumFlagsField(label, memberVal, options);
            return EditorGUILayout.EnumPopup(label, memberVal, options);
        }

        protected override Enum DrawValue(Rect rect, GUIContent label, Enum memberVal)
        {
            if (HasFlags)
                return EditorGUI.EnumFlagsField(rect, label, memberVal);
            return EditorGUI.EnumPopup(rect, label, memberVal);
        }
    }
}