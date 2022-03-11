using System;
using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    [DontApplyToListElements]
    public class OrderRelativeToAttribute : ShowInInspectorAttribute
    {
        public string Member;
        public int OrderAfterMember;

        /// <summary>
        /// Draws a property after the specified member.
        /// </summary>
        /// <param name="MemberName">The name of the member to which this property will be placed after.</param>
        public OrderRelativeToAttribute(string MemberName) {
            Member = MemberName;
            OrderAfterMember = 1;
        }

        /// <summary>
        /// Draws a property in an order relative to a member.
        /// </summary>
        /// <param name="MemberName">The name of the member to which this property's ordering will be adjusted relatively.</param>
        /// <param name="AdditionalOrder">The relative position of the property (-9 to 9).</param>
        public OrderRelativeToAttribute(string MemberName, int AdditionalOrder) {
            Member = MemberName;
#if UNITY_EDITOR
            if (Mathf.Abs(AdditionalOrder) > 9) {
                UnityEngine.Debug.LogWarning("Max Additional Order for attributes is 9.");
                AdditionalOrder = 9 * (int) Mathf.Sign(AdditionalOrder);
            }
#endif
            OrderAfterMember = AdditionalOrder;
        }
    }
    
    [Conditional("UNITY_EDITOR")]
    public class OrderBeforeAttribute : OrderRelativeToAttribute
    {
        public OrderBeforeAttribute(string MemberName) : base(MemberName, -1)
        {
        }
    }
    
    [Conditional("UNITY_EDITOR")]
    public class OrderAfterAttribute : OrderRelativeToAttribute
    {
        public OrderAfterAttribute(string MemberName) : base(MemberName, 1)
        {
        }
    }
}