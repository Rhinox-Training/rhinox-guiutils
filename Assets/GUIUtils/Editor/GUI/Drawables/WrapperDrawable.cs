using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class WrapperDrawable : BaseDrawable
    {
        protected IOrderedDrawable _innerDrawable;

        public override string LabelString => string.Empty; // Not used in wrapper
        
        protected WrapperDrawable(IOrderedDrawable drawable)
        {
            _innerDrawable = drawable;
            Host = _innerDrawable.Host;
        }
        
        protected override void DrawInner(GUIContent label)
        {
            _innerDrawable.Draw();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            _innerDrawable.Draw(rect);
        }
    }
}