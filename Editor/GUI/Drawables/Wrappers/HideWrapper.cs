using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HideWrapper : WrapperDrawable
    {
        private bool _state;

        private IPropertyMemberHelper<bool> _stateMember;

        public HideWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label)
        {
            if (ShouldDraw())
                base.DrawInner(label);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (ShouldDraw())
                base.DrawInner(rect, label);
        }

        protected bool ShouldDraw()
        {
            if (_stateMember == null)
                return _state;
            
            return _stateMember.GetValue() == _state;
        }

        [WrapDrawer(typeof(ShowIfAttribute), -11000)]
        public static WrapperDrawable Create(ShowIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = PropertyMemberHelper.Create<bool>(drawable.Host, attr.MemberName);
            return new HideWrapper(drawable)
            {
                _state = true,
                _stateMember = member
            };
        }
        
        [WrapDrawer(typeof(HideIfAttribute), -11000)]
        public static WrapperDrawable Create(HideIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = PropertyMemberHelper.Create<bool>(drawable.Host, attr.MemberName);
            return new HideWrapper(drawable)
            {
                _state = false,
                _stateMember = member
            };
        }
        
        [WrapDrawer(typeof(HideInInspector), -11000)]
        public static WrapperDrawable Create(HideInInspector attr, IOrderedDrawable drawable)
        {
            return new HideWrapper(drawable)
            {
                _state = false
            };
        }
    }
}