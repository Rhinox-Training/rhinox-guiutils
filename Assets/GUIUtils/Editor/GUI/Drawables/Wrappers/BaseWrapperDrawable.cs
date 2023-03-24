using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseWrapperDrawable : BaseDrawable
    {
        protected IOrderedDrawable _innerDrawable;

        public override float ElementHeight => _innerDrawable.ElementHeight;

        public override bool IsVisible => _innerDrawable.IsVisible;

        public override string LabelString => string.Empty; // Not used in wrapper

        protected BaseWrapperDrawable(IOrderedDrawable drawable)
        {
            if (drawable == null) throw new ArgumentNullException(nameof(drawable));
            _innerDrawable = drawable;
            Host = _innerDrawable.Host;
            Order = _innerDrawable.Order;
        }

        public override ICollection<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            return _innerDrawable.GetDrawableAttributes<TAttribute>();
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