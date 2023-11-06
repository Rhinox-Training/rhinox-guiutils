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
            if (rect.IsValid() && _cachedRect != rect)
            {
                _cachedRect = rect;
                RequestRepaint();
            }
            else
            {
                rect = _cachedRect;
                rect.xMax -= CustomGUIUtility.Padding;
            }
            
            GUILayout.BeginArea(_cachedRect);
            var contentRect = EditorGUILayout.BeginVertical();
            Draw(label);
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            if (!contentRect.height.LossyEquals(_cachedRect.height))
            {
                _cachedRect.height = contentRect.height;
                RequestRepaint();
            }
        }

        private void Draw(GUIContent label)
        {
            _methodMember?.DrawError();
            
            var value = GetValue();
            var newValue = _methodMember?.Invoke(value, label);
            if (!Equals(value, newValue) && SetValue(newValue))
                GUI.changed = true;
        }

        [WrapDrawer(typeof(CustomValueDrawerAttribute), Priority.Simple)]
        public static BaseWrapperDrawable Create(CustomValueDrawerAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.CreateMethod(drawable.HostInfo, attr.MethodName);
            member.EnforceSyntax(parameterIndex: 1, typeof(GUIContent));
            member.EnforceSyntax(numberOfParameters: 2);
            member.EnforceSyntax(hasReturnType: true);
            
            var hostedReturnType = drawable.HostInfo.GetReturnType();
            if (hostedReturnType.InheritsFrom<IEnumerable>())
            {
                var elementType = hostedReturnType.GetCollectionElementType();
                if (member.MethodReturnType == elementType)
                    return null;
            }

            return new CustomValueDrawerWrapper(drawable)
            {
                _methodMember = member
            };
        }
    }
}