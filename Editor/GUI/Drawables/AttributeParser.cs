using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class AttributeParser
    {
        public static bool TryParseOrder(MemberInfo memberInfo, out int order, int defaultReturn = 0)
        {
            var orderAttr = AttributeProcessorHelper.FindAttributeInclusive<PropertyOrderAttribute>(memberInfo);
            if (orderAttr != null)
            {
                order = (int)orderAttr.Order;
                return true;
            }
            
            order = defaultReturn;
            return false;
        }

        public static bool ParseDrawAsUnity(MemberInfo memberInfo)
        {
            var attr = AttributeProcessorHelper.FindAttributeInclusive<DrawAsUnityObjectAttribute>(memberInfo);
            return attr != null;
        }

        public static void Parse(MemberInfo memberInfo, ref IOrderedDrawable drawable)
        {
            if (TryParseOrder(memberInfo, out int order))
                drawable.Order = order;
        }
    }
}