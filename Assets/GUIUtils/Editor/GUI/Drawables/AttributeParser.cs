using System;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class AttributeParser
    {
        public static bool ParseDrawAsUnity(MemberInfo memberInfo, Type hostType = null)
        {
            var attr = AttributeProcessorHelper.FindAttributeInclusive<DrawAsUnityObjectAttribute>(memberInfo, hostType);
            return attr != null;
        }
    }
}