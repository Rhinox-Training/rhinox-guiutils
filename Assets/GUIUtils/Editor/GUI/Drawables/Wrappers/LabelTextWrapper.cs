using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
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
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _stringHelper.DrawError();

            var text = _stringHelper.GetSmartValue();
            base.DrawInner(new GUIContent(text), options);
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var text = _stringHelper.GetSmartValue();
            base.DrawInner(rect, new GUIContent(text));
            _stringHelper.DrawError(rect);
        }
        
        [WrapDrawer(typeof(LabelTextAttribute), -1000)]
        public static BaseWrapperDrawable Create(LabelTextAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<string>(drawable.HostInfo, attr.Text);
            return new LabelTextWrapper(drawable)
            {
                _stringHelper = member
            };
        }
        
        [WrapDrawer(typeof(FittedLabelAttribute), -1000)]
        public static BaseWrapperDrawable Create(FittedLabelAttribute attr, IOrderedDrawable drawable)
        {
            if (attr.Text.IsNullOrEmpty())
                return null;
            
            var member = MemberHelper.Create<string>(drawable.HostInfo, attr.Text);
            return new LabelTextWrapper(drawable)
            {
                _stringHelper = member
            };
        }
    }
}