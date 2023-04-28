using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class InlineButtonWrapper : BaseWrapperDrawable
    {
        protected MethodMemberHelper _methodHelper;
        protected string _label;
        
        public InlineButtonWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            base.DrawInner(label, options);
            if (GUILayout.Button(GetContent(), GUILayout.ExpandWidth(false), GUILayout.MaxHeight(ElementHeight)))
                Invoke();
            GUILayout.EndHorizontal();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            GUI.skin.label.CalcMinMaxWidth(_label, out float min, out float max);

            var buttonRect = rect.AlignRight(max);
            rect.xMax -= max;
            base.DrawInner(rect, label);
            if (GUI.Button(buttonRect, GetContent()))
                Invoke();
        }

        protected virtual GUIContent GetContent()
        {
            return GUIContentHelper.TempContent(_label);
        }

        protected virtual void Invoke()
        {
            _methodHelper?.Invoke();
        }

        [WrapDrawer(typeof(InlineButtonAttribute), -5000)]
        public static BaseWrapperDrawable Create(InlineButtonAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.CreateMethod(drawable.HostInfo, attr.MemberMethod);
            return new InlineButtonWrapper(drawable)
            {
                _label = attr.Label,
                _methodHelper = member
            };
        }
    }
}