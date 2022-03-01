using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Odin
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class FoldoutContainerAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// Gets a value indicating whether or not the foldout should be expanded by default..
        /// </summary>
        public bool Expanded;
        
        /// <summary>Adds the property to the specified foldout group.</summary>
        /// <param name="groupName">Name of the foldout group.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public FoldoutContainerAttribute(string groupName, int order = 0)
            : base(groupName, order)
        {
        }
    
        /// <summary>Adds the property to the specified foldout group.</summary>
        /// <param name="groupName">Name of the foldout group.</param>
        /// <param name="expanded">Whether or not the foldout should be expanded by default.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public FoldoutContainerAttribute(string groupName, bool expanded, int order = 0)
            : base(groupName, order)
        {
            this.Expanded = expanded;
            this.HasDefinedExpanded = true;
        }
    
        /// <summary>
        /// Gets a value indicating whether or not the Expanded property has been set.
        /// </summary>
        public bool HasDefinedExpanded { get; private set; }
    
        /// <summary>Combines the foldout property with another.</summary>
        /// <param name="other">The group to combine with.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            FoldoutContainerAttribute foldoutGroupAttribute = other as FoldoutContainerAttribute;
            if (foldoutGroupAttribute.HasDefinedExpanded)
            {
                this.HasDefinedExpanded = true;
                this.Expanded = foldoutGroupAttribute.Expanded;
            }
            if (!this.HasDefinedExpanded)
                return;
            foldoutGroupAttribute.HasDefinedExpanded = true;
            foldoutGroupAttribute.Expanded = this.Expanded;
        }
    }

}
