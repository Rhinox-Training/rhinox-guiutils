using System;
using System.Diagnostics;

namespace Rhinox.GUIUtils.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class AssignableTypeFilterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type that the filter will use to find out inherited types for the dropdown.
        /// </summary>
        public Type BaseType;
        
        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;

        public bool Expanded = false;

        public AssignableTypeFilterAttribute()
        {
            
        }

        public AssignableTypeFilterAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
