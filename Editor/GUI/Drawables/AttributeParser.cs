using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class AttributeParser
    {
        public static bool TryParseOrder(MemberInfo memberInfo, out int order, int defaultReturn = 0)
        {
            var orderAttr = memberInfo.GetCustomAttribute<PropertyOrderAttribute>();
            if (orderAttr != null)
            {
                order = orderAttr.Order;
                return true;
            }
            
            order = defaultReturn;
            return false;
        }

        public static bool ParseDrawAsUnity(MemberInfo memberInfo)
        {
            var attr = memberInfo.GetCustomAttribute<DrawAsUnityObjectAttribute>();
            return attr != null;
        }

        public static void Parse(MemberInfo memberInfo, ref IOrderedDrawable drawable)
        {
            if (TryParseOrder(memberInfo, out int order))
                drawable.Order = order;
            
        }
    }
}