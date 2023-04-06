using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ButtonGroupDrawable : BaseHorizontalGroupDrawable<ButtonGroupAttribute>
    {
        public ButtonGroupDrawable(GroupedDrawable parent, string groupID, int order) : base(parent, groupID, order)
        {
        }

        protected override void ParseAttribute(IOrderedDrawable child, ButtonGroupAttribute attr)
        {
        }

        protected override void ParseAttribute(ButtonGroupAttribute attr)
        {
        }
    }
}