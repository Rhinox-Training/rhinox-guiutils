using System;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public class VerticalGroupDrawable : BaseVerticalGroupDrawable<VerticalGroupAttribute>
    {
        public VerticalGroupDrawable()
            : this(null, null, 0)
        {
            
        }
        
        public VerticalGroupDrawable(GroupedDrawable parent, string groupID, float order)
            : base(parent, groupID, order)
        {
        }

        protected override void ParseAttribute(IOrderedDrawable child, VerticalGroupAttribute attr)
        {
            // TODO
        }

        protected override void ParseAttribute(VerticalGroupAttribute attr)
        {
            SetOrder(attr.Order);
            
            // TODO

            _parent?.EnsureSizeFits(_size);
        }
    }
}