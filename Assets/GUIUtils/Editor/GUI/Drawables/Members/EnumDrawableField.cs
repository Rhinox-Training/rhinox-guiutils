using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class EnumDrawableField: BaseDrawable<Enum>
    {
        public bool HasFlags { get; }
        
        public EnumDrawableField(object instance, MemberInfo info) : base(instance, info)
        {
            HasFlags = info.GetReturnType().GetCustomAttribute<FlagsAttribute>() != null;
        }

        protected override Enum DrawValue(object instance, Enum memberVal)
        {
            if (HasFlags)
                return EditorGUILayout.EnumFlagsField(memberVal);
            return EditorGUILayout.EnumPopup(memberVal);
        }

        protected override Enum DrawValue(Rect rect, object instance, Enum memberVal)
        {
            if (HasFlags)
                return EditorGUI.EnumFlagsField(rect, memberVal);
            return EditorGUI.EnumPopup(rect, memberVal);
        }
    }
}