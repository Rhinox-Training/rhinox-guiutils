using System.Collections;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class CustomValueDrawerWrapper : BaseWrapperDrawable
    {
        private MethodMemberHelper _methodMember;

        private Rect _cachedRect;

        public override float ElementHeight => _cachedRect.IsValid() ? _cachedRect.height : base.ElementHeight;

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
            if (rect.IsValid())
                _cachedRect = rect;
            
            GUILayout.BeginArea(_cachedRect, CustomGUIStyles.Clean);
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
            
            if (drawable is IHostedDrawable hostedDrawable)
            {
                var hostedReturnType = hostedDrawable.HostInfo.GetReturnType();
                if (hostedReturnType.InheritsFrom<IEnumerable>())
                {
                    var elementType = hostedReturnType.GetCollectionElementType();
                    if (member.MethodReturnType == elementType)
                        return null;
                }
            }

            return new CustomValueDrawerWrapper(drawable)
            {
                _methodMember = member
            };
        }
    }
}