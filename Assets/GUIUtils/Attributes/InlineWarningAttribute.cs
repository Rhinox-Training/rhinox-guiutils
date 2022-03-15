using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class InlineWarningAttribute : Attribute
    {
        public string Text { get; private set; }
    
        /// <summary>The name of a bool member field, property or method.</summary>
        public string MemberName;
        /// <summary>The optional member value.</summary>
        public object Value;
    
        /// <summary>
        /// Enables a property in the inspector, based on the state of a member.
        /// </summary>
        /// <param name="memberName">Name of member bool field, property, or method.</param>
        public InlineWarningAttribute(string text, string memberName)
        {
            this.Text = text;
            this.MemberName = memberName;
        }
    
        /// <summary>
        /// Enables a property in the inspector, if the specified member returns the specified value.
        /// </summary>
        /// <param name="memberName">Name of member to check value of.</param>
        /// <param name="optionalValue">Value to check against.</param>
        public InlineWarningAttribute(string text, string memberName, object optionalValue)
        {
            this.Text = text;
            this.MemberName = memberName;
            this.Value = optionalValue;
        }
    }

}
