using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class PropertyGroupDrawable<T> : GroupedDrawable
        where T : PropertyGroupAttribute
    {
        protected readonly List<T> _parsedAttributes = new List<T>();

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

            // No need to trigger ParseAttribute -> should have happened already, but it can't hurt
            ParseAttribute(groupAttribute);
            ParseAttribute(child, groupAttribute);
        }

        protected abstract void ParseAttribute(IOrderedDrawable child, T attr);

        
        protected override void ParseAttribute(PropertyGroupAttribute attr)
        {
            if (_parsedAttributes.Contains(attr))
                return;

            var groupAttribute = ValidateAttribute(attr);
            _parsedAttributes.Add(groupAttribute);
            ParseAttribute(groupAttribute);
        }
        
        protected abstract void ParseAttribute(T attr);

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            return _parsedAttributes.OfType<TAttribute>();
        }
    }
}