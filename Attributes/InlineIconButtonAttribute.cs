using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class InlineIconButtonAttribute : Attribute
    {
        public string Icon;
        public string MethodName;
        public string Tooltip;
        public bool ForceEnable;
    
        /// <summary>
        /// Enables a property in the inspector, based on the state of a member.
        /// </summary>
        /// <param name="memberName">Name of member bool field, property, or method.</param>
        public InlineIconButtonAttribute(string icon, string methodName)
        {
            this.Icon = icon;
            this.MethodName = methodName;
        }
    }
    
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class InlineListElementIconButtonAttribute : Attribute
    {
        public string Icon;
        public string MethodName;
        public string Tooltip;
        public bool ForceEnable;

        /// <summary>
        /// Enables a property in the inspector, based on the state of a member.
        /// </summary>
        /// <param name="memberName">Name of member bool field, property, or method.</param>
        public InlineListElementIconButtonAttribute(string icon, string methodName)
        {
            this.Icon = icon;
            this.MethodName = methodName;
        }
    }

}
