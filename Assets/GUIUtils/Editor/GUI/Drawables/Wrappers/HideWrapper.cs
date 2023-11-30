using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HideWrapper : BaseWrapperDrawable
    {
        private bool _state;

        private IPropertyMemberHelper _stateMember;
        public object _stateMemberValue;

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

            var value = _stateMember.GetValue();
            if (_stateMemberValue != null)
                value = object.Equals(value, _stateMemberValue);
            else if (!(value is bool))
                value = value == null;
            return (bool) (value ?? false) == _state;
        }

        [WrapDrawer(typeof(ShowIfAttribute), Priority.BehaviourPrevention)]
        public static BaseWrapperDrawable Create(ShowIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<object>(drawable.HostInfo, attr.MemberName);
            return new HideWrapper(drawable)
            {
                _state = true,
                _stateMember = member,
                _stateMemberValue = attr.Value
            };
        }
        
        [WrapDrawer(typeof(HideIfAttribute), Priority.BehaviourPrevention)]
        public static BaseWrapperDrawable Create(HideIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<object>(drawable.HostInfo, attr.MemberName);
            return new HideWrapper(drawable)
            {
                _state = false,
                _stateMember = member,
                _stateMemberValue = attr.Value
            };
        }
        
        [WrapDrawer(typeof(HideInInspector), Priority.BehaviourPrevention)]
        public static BaseWrapperDrawable Create(HideInInspector attr, IOrderedDrawable drawable)
        {
            return new HideWrapper(drawable)
            {
                _state = false
            };
        }
    }
}