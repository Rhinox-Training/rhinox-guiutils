using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ValueChangedWrapper : WrapperDrawable
    {
        private MethodMemberHelper _methodMember;
        
        public ValueChangedWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            EditorGUI.BeginChangeCheck();
            base.OnPreDraw();
        }

        protected override void OnPostDraw()
        {
            base.OnPostDraw();
            if (EditorGUI.EndChangeCheck())
                _methodMember.Invoke();
        }
        
        [WrapDrawer(typeof(OnValueChangedAttribute), -500)]
        public static WrapperDrawable Create(OnValueChangedAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.CreateMethod(drawable.Host, attr.MethodName);
            return new ValueChangedWrapper(drawable)
            {
                _methodMember = member
            };
        }
    }
    
}