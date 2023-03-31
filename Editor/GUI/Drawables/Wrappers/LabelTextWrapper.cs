using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class LabelTextWrapper : BaseWrapperDrawable
    {
        private IPropertyMemberHelper<string> _stringHelper;

        public LabelTextWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var text = _stringHelper.GetValue();
            base.DrawInner(rect, new GUIContent(text));
        }

        protected override void DrawInner(GUIContent label)
        {
            var text = _stringHelper.GetValue();
            base.DrawInner(new GUIContent(text));
        }
        
        [WrapDrawer(typeof(LabelTextAttribute), -1000)]
        public static BaseWrapperDrawable Create(LabelTextAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<string>(drawable.Host, attr.Text);
            return new LabelTextWrapper(drawable)
            {
                _stringHelper = member
            };
        }
        
        [WrapDrawer(typeof(FittedLabelAttribute), -1000)]
        public static BaseWrapperDrawable Create(FittedLabelAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<string>(drawable.Host, attr.Text);
            return new LabelTextWrapper(drawable)
            {
                _stringHelper = member
            };
        }
    }
}