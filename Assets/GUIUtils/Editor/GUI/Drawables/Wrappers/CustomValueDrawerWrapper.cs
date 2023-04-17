using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class CustomValueDrawerWrapper : BaseWrapperDrawable
    {
        private MethodMemberHelper _methodMember;
        
        public CustomValueDrawerWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(CustomGUIStyles.Clean, options);
            Draw(label);
            GUILayout.EndVertical();
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            GUILayout.BeginArea(rect);
            Draw(label);
            GUILayout.EndArea();
        }

        private void Draw(GUIContent label)
        {
            _methodMember?.DrawError();
            
            var value = GetValue();
            var newValue = _methodMember?.Invoke(value, label);
            if (!Equals(value, newValue) && SetValue(newValue))
                GUI.changed = true;
        }

        [WrapDrawer(typeof(CustomValueDrawerAttribute), -1)]
        public static BaseWrapperDrawable Create(CustomValueDrawerAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.CreateMethod(drawable.Host, attr.MethodName);
            member.EnforceSyntax(parameterIndex: 1, typeof(GUIContent));
            member.EnforceSyntax(numberOfParameters: 2);
            member.EnforceSyntax(hasReturnType: true);

            return new CustomValueDrawerWrapper(drawable)
            {
                _methodMember = member
            };
        }
    }
}