using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    internal class OrderRelativeToAttributePropertyProcessor : OdinPropertyProcessor
    {
        List<InspectorPropertyInfo> _propertiesToBeReordered = new List<InspectorPropertyInfo>();

        public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
        {
            _propertiesToBeReordered.Clear();

            foreach (InspectorPropertyInfo mInfo in memberInfos)
            {
                if (mInfo.GetAttribute<OrderRelativeToAttribute>() != null)
                    _propertiesToBeReordered.Add(mInfo);
            }

            if (_propertiesToBeReordered.Count == 0)
                return;
            
            for (int i = 0; i < memberInfos.Count; i++)
                memberInfos[i].Order = 10 * i;

            foreach (InspectorPropertyInfo propInfo in _propertiesToBeReordered)
            {
                OrderRelativeToAttribute attr = propInfo.GetAttribute<OrderRelativeToAttribute>();
                bool memberFound = false;
                for (int i = 0; i < memberInfos.Count; i++)
                {
                    if (memberInfos[i].PropertyName == attr.Member)
                    {
                        propInfo.Order = memberInfos[i].Order + attr.OrderAfterMember;

                        PropertyOrderAttribute targetsPropertyOrderAttribute = propInfo.GetAttribute<PropertyOrderAttribute>();
                        if (targetsPropertyOrderAttribute != null)
                        {
                            var orderAttr = new PropertyOrderAttribute(targetsPropertyOrderAttribute.Order);
                            propInfo.GetEditableAttributesList().Add(orderAttr);
                        }

                        memberFound = true;
                        break;
                    }
                }

                if (!memberFound)
                    Debug.LogError(
                        $"[{typeof(OrderRelativeToAttribute)}({propInfo.PropertyName})]: " +
                        $"Couldn't find member with name {attr.Member}."); 
            }
        }

    }
}