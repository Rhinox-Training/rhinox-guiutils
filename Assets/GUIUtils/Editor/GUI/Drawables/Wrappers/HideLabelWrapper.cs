using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HideLabelWrapper : BaseWrapperDrawable
    {
        public HideLabelWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            base.DrawInner(GUIContent.none, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            base.DrawInner(rect, GUIContent.none);
        }

        [WrapDrawer(typeof(HideLabelAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(HideLabelAttribute attr, IOrderedDrawable drawable)
        {
            return new HideLabelWrapper(drawable)
            {
            };
        }
    }
}