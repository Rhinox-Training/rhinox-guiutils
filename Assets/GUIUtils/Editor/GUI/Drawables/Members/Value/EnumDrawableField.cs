using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class EnumDrawableField: BaseMemberValueDrawable<Enum>
    {
        public bool HasFlags { get; }
        public bool HasToggleButtons { get; }
        
        public EnumDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
            HasFlags = AttributeProcessorHelper.FindAttributeInclusive<FlagsAttribute>(hostInfo.GetReturnType()) != null;
            HasToggleButtons = hostInfo.GetAttribute<EnumToggleButtonsAttribute>() != null;
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