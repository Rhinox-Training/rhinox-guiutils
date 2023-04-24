using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ValueChangedWrapper : BaseWrapperDrawable
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

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _methodMember?.DrawError();
            base.DrawInner(label, options);
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            base.DrawInner(rect, label);
            _methodMember?.DrawError(rect);
        }

        [WrapDrawer(typeof(OnValueChangedAttribute), -500)]
        public static BaseWrapperDrawable Create(OnValueChangedAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.CreateMethod(drawable.HostInfo, attr.MethodName);
            return new ValueChangedWrapper(drawable)
            {
                _methodMember = member
            };
        }
    }
    
}