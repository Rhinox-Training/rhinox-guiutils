using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class PropertyGroupDrawable<T> : GroupedDrawable
        where T : PropertyGroupAttribute
    {
        protected List<T> _groupAttributes = new List<T>();
        
        protected PropertyGroupDrawable(GroupedDrawable parent, string groupID, float order) : base(parent, groupID, order)
        {
            
        }
        
        protected T ValidateAttribute(PropertyGroupAttribute attr)
        {
            if (attr is T typedAttr)
                return typedAttr;
            
            // incorrect attribute
            throw new ArgumentException(nameof(attr), $"{GetType().Name} cannot parse attribute of type {attr.GetType().Name}");
        }

        protected override void ParseAttribute(IOrderedDrawable child, PropertyGroupAttribute attr)
        {
            var groupAttribute = ValidateAttribute(attr);

            _groupAttributes.Add(groupAttribute);
            
            // No need to trigger ParseAttribute -> should have happened already, but it can't hurt
            ParseAttribute(groupAttribute);
            ParseAttribute(child, groupAttribute);
        }

        protected abstract void ParseAttribute(IOrderedDrawable child, T attr);
        protected abstract void ParseAttribute(T attr);

        public override void AddAttribute(Attribute attr)
        {
            if (_attributes.Contains(attr))
                return;
            
            base.AddAttribute(attr);

            if (attr is PropertyGroupAttribute groupAttribute)
            {
                var typedGroupAttribute = ValidateAttribute(groupAttribute);
                ParseAttribute(typedGroupAttribute);
            }
        }
    }
}