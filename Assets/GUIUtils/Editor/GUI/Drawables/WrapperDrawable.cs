using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class WrapperDrawable : BaseDrawable
    {
        protected IOrderedDrawable _innerDrawable;

        public override float ElementHeight => _innerDrawable.ElementHeight;

        public override string LabelString => string.Empty; // Not used in wrapper
        
        protected WrapperDrawable(IOrderedDrawable drawable)
        {
            _innerDrawable = drawable;
            Host = _innerDrawable.Host;
            Order = _innerDrawable.Order;
        }
        
        protected override void DrawInner(GUIContent label)
        {
            OnPreDraw();
            
            _innerDrawable.Draw();

            OnPostDraw();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            OnPreDraw();

            _innerDrawable.Draw(rect);
            
            OnPostDraw();
        }
        
        protected virtual void OnPreDraw()
        {
        }
        
        protected virtual void OnPostDraw()
        {
        }
    }
}