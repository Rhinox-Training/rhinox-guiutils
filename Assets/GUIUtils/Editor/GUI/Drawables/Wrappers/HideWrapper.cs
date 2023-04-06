using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HideWrapper : BaseWrapperDrawable
    {
        private bool _state;

        private IPropertyMemberHelper<bool> _stateMember;

        public override bool IsVisible => _innerDrawable.IsVisible && ShouldDraw();

        public HideWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _stateMember?.DrawError();
            if (ShouldDraw())
                base.DrawInner(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (ShouldDraw())
                base.DrawInner(rect, label);
            
            _stateMember?.DrawError(rect);
        }

        public bool ShouldDraw()
        {
            if (_stateMember == null)
                return _state;
            
            return _stateMember.GetValue() == _state;
        }

        [WrapDrawer(typeof(ShowIfAttribute), -11000)]
        public static BaseWrapperDrawable Create(ShowIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<bool>(drawable.Host, attr.MemberName);
            return new HideWrapper(drawable)
            {
                _state = true,
                _stateMember = member
            };
        }
        
        [WrapDrawer(typeof(HideIfAttribute), -11000)]
        public static BaseWrapperDrawable Create(HideIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<bool>(drawable.Host, attr.MemberName);
            return new HideWrapper(drawable)
            {
                _state = false,
                _stateMember = member
            };
        }
        
        [WrapDrawer(typeof(HideInInspector), -11000)]
        public static BaseWrapperDrawable Create(HideInInspector attr, IOrderedDrawable drawable)
        {
            return new HideWrapper(drawable)
            {
                _state = false
            };
        }
    }
}