using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class OverrideGroupAttribute : PropertyGroupAttribute
    {
        public string ToggleMemberName;

        public string HideIfMemberName;
        
        public OverrideGroupAttribute(string toggleMemberName, string groupId, int order, string hideIfMemberName = null) : base(groupId, order)
        {
            ToggleMemberName = toggleMemberName;
            HideIfMemberName = hideIfMemberName;
        }

        public OverrideGroupAttribute(string toggleMemberName, string groupId, string hideIfMemberName = null) : base(groupId)
        {
            ToggleMemberName = toggleMemberName;
            HideIfMemberName = hideIfMemberName;
        }
        
        public OverrideGroupAttribute(string toggleMemberName, string hideIfMemberName = null) : base(toggleMemberName)
        {
            ToggleMemberName = toggleMemberName;
            HideIfMemberName = hideIfMemberName;
        }
        
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            OverrideGroupAttribute otherGroupAttribute = other as OverrideGroupAttribute;
            if (ToggleMemberName == null)
                ToggleMemberName = otherGroupAttribute.ToggleMemberName;
            else if (otherGroupAttribute.ToggleMemberName == null)
                otherGroupAttribute.ToggleMemberName = this.ToggleMemberName;
            
            if (HideIfMemberName == null)
                HideIfMemberName = otherGroupAttribute.HideIfMemberName;
            else if (otherGroupAttribute.HideIfMemberName == null)
                otherGroupAttribute.HideIfMemberName = this.HideIfMemberName;
        }
    }
}