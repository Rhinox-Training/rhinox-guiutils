using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GUIStateWrapper : BaseWrapperDrawable
    {
        private bool _state;
        private bool _previousState;

        private IPropertyMemberHelper _stateMember;
        public object _stateMemberValue;

        public GUIStateWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            _stateMember?.DrawError();
            
            _previousState = GUI.enabled;

            if (GUI.enabled && ShouldDisable())
                GUI.enabled = false;
            
            base.OnPreDraw();
        }

        protected override void OnPostDraw()
        {
            base.OnPostDraw();

            GUI.enabled = _previousState;
        }

        protected bool ShouldDisable()
        {
            if (_stateMember == null)
                return !_state;

            var value = _stateMember.GetValue();
            if (_stateMemberValue != null)
                value = object.Equals(value, _stateMemberValue);
            else if (!(value is bool))
                value = value == null;
            return  (bool) (value ?? false) != _state;
        }

        [WrapDrawer(typeof(EnableIfAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(EnableIfAttribute attr, IOrderedDrawable drawable)
        { 
            var member = MemberHelper.Create<object>(drawable.HostInfo, attr.MemberName);
            return new GUIStateWrapper(drawable)
            {
                _state = true,
                _stateMember = member,
                _stateMemberValue = attr.Value
            };
        }
        
        [WrapDrawer(typeof(DisableIfAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(DisableIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<object>(drawable.HostInfo, attr.MemberName);
            
            return new GUIStateWrapper(drawable)
            {
                _state = false,
                _stateMember = member,
                _stateMemberValue = attr.Value
            };
        }
        
        [WrapDrawer(typeof(ReadOnlyAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(ReadOnlyAttribute attr, IOrderedDrawable drawable)
        {
            return new GUIStateWrapper(drawable)
            {
                _state = false
            };
        }
    }
}