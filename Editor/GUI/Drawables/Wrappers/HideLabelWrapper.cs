using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HideLabelWrapper : BaseWrapperDrawable
    {
        public HideLabelWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            base.DrawInner(rect, GUIContent.none);
        }

        protected override void DrawInner(GUIContent label)
        {
            base.DrawInner(GUIContent.none);
        }

        [WrapDrawer(typeof(HideLabelAttribute), -8000)]
        public static BaseWrapperDrawable Create(HideLabelAttribute attr, IOrderedDrawable drawable)
        {
            return new HideLabelWrapper(drawable)
            {
            };
        }
    }
}