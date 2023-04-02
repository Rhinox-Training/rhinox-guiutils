using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GUIStateWrapper : BaseWrapperDrawable
    {
        private bool _state;
        private bool _previousState;

        private IPropertyMemberHelper<bool> _stateMember;

        public GUIStateWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            _stateMember.DrawError();
            
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
            
            return _stateMember.GetValue() != _state;
        }

        [WrapDrawer(typeof(EnableIfAttribute), -10500)]
        public static BaseWrapperDrawable Create(EnableIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<bool>(drawable.Host, attr.MemberName);
            return new GUIStateWrapper(drawable)
            {
                _state = true,
                _stateMember = member
            };
        }
        
        [WrapDrawer(typeof(DisableIfAttribute), -10500)]
        public static BaseWrapperDrawable Create(DisableIfAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<bool>(drawable.Host, attr.MemberName);
            return new GUIStateWrapper(drawable)
            {
                _state = false,
                _stateMember = member
            };
        }
        
        [WrapDrawer(typeof(ReadOnlyAttribute), -10500)]
        public static BaseWrapperDrawable Create(ReadOnlyAttribute attr, IOrderedDrawable drawable)
        {
            return new GUIStateWrapper(drawable)
            {
                _state = false
            };
        }
    }
}