using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class SpaceWrapper : BaseWrapperDrawable
    {
        private float _amountBefore;
        private float _amountAfter;

        public override float ElementHeight => base.ElementHeight + _amountAfter + _amountBefore;

        public SpaceWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            rect.yMin += _amountBefore;
            rect.yMax -= _amountAfter;
            base.DrawInner(rect, label);

        }

        protected override void DrawInner(GUIContent label)
        {
            if (_amountBefore > 0)
                GUILayout.Space(_amountBefore);
            base.DrawInner(label);
            if (_amountAfter > 0)
                GUILayout.Space(_amountAfter);
        }

        [WrapDrawer(typeof(SpaceAttribute))]
        public static BaseWrapperDrawable Create(SpaceAttribute attr, IOrderedDrawable drawable)
        {
            return new SpaceWrapper(drawable)
            {
                _amountBefore = attr.height
            };
        }
        
        [WrapDrawer(typeof(PropertySpaceAttribute))]
        public static BaseWrapperDrawable Create(PropertySpaceAttribute attr, IOrderedDrawable drawable)
        {
            return new SpaceWrapper(drawable)
            {
                _amountBefore = attr.SpaceBefore,
                _amountAfter = attr.SpaceAfter
            };
        }
    }
}