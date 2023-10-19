using Sirenix.OdinInspector;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class SuffixLabelWrapper : BaseWrapperDrawable
    {
        private GUIStyle _style;
        
        private IPropertyMemberHelper<string> _labelMember;
        private bool _overlay;
        
        public SuffixLabelWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            if (_style == null)
                _style = CustomGUIStyles.UnpaddedLabel;
            
            base.OnPreDraw();
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(CustomGUIStyles.Clean);
            _labelMember.DrawError();

            var text = _labelMember.GetSmartValue();
            base.DrawInner(label, options);
            
            _style.CalcMinMaxWidth(text, out float min, out float max);
            GUILayout.Label(GUIContentHelper.TempContent(text), _style, GUILayout.Width(max));
            GUILayout.EndHorizontal();
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var text = _labelMember.GetSmartValue();
            
            _style.CalcMinMaxWidth(text, out float min, out float max);
            var suffixRect = rect.AlignRight(max + CustomGUIUtility.Padding);
            if (!_overlay)
                rect = rect.PadRight(max + CustomGUIUtility.Padding * 2);
            
            base.DrawInner(rect, label);
            
            GUI.Label(suffixRect, GUIContentHelper.TempContent(text), _style);
            
            _labelMember.DrawError(rect);
        }
        
        [WrapDrawer(typeof(SuffixLabelAttribute), -500)]
        public static BaseWrapperDrawable Create(SuffixLabelAttribute attr, IOrderedDrawable drawable)
        {
            if (attr.Label.IsNullOrEmpty())
                return null;
            
            var member = MemberHelper.Create<string>(drawable.HostInfo, attr.Label);
            return new SuffixLabelWrapper(drawable)
            {
                _labelMember = member,
                _overlay = attr.Overlay
            };
        }
    }
}