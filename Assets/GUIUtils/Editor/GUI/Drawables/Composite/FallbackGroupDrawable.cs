using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public class FallbackGroupDrawable : BaseVerticalGroupDrawable<PropertyGroupAttribute>
    {
        public FallbackGroupDrawable(GroupedDrawable parent, string groupID, float order) 
            : base(parent, groupID, order)
        {
        }


        protected override void ParseAttributeSmart(IOrderedDrawable child, PropertyGroupAttribute attr)
        {
            // NOP
        }

        protected override void ParseAttributeSmart(PropertyGroupAttribute attr)
        {
            SetOrder(attr.Order);
            
            // TODO
            _parent?.EnsureSizeFits(_size);
        }
    }
}