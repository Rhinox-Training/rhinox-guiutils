using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseWrapperDrawable : BaseDrawable
    {
        protected IOrderedDrawable _innerDrawable;

        public override float ElementHeight => _innerDrawable.ElementHeight;

        public override bool IsVisible => _innerDrawable.IsVisible;

        public override GUIContent Label => _innerDrawable.Label;

        public override string LabelString => string.Empty; // Not used in wrapper

        protected BaseWrapperDrawable(IOrderedDrawable drawable)
        {
            if (drawable == null) throw new ArgumentNullException(nameof(drawable));
            _innerDrawable = drawable;
            Host = _innerDrawable.Host;
            Order = _innerDrawable.Order;
        }

        private bool _localShouldRepaint;
        public override bool ShouldRepaint
        {
            get
            {
                return _localShouldRepaint || _innerDrawable.ShouldRepaint;
            }
            protected set
            {
                _localShouldRepaint = value;
            }
        }

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            return _innerDrawable.GetDrawableAttributes<TAttribute>();
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _innerDrawable.Draw(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            _innerDrawable.Draw(rect, label);
        }

        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            _localShouldRepaint = false;
        }
        
        
        protected object GetValue()
        {
            if (_innerDrawable is IMemberDrawable memberDrawable)
                return memberDrawable.Entry.GetValue();
            
            if (_innerDrawable is IObjectDrawable && _innerDrawable.Host is GenericMemberEntry entry)
                return entry.GetValue();

            return null;
        }
        protected bool SetValue(object value)
        {
            if (_innerDrawable is IMemberDrawable memberDrawable)
                return memberDrawable.Entry.TrySetValue(value);
            
            if (_innerDrawable is IObjectDrawable && _innerDrawable.Host is GenericMemberEntry entry)
                return entry.TrySetValue(value);

            return false;
        }
        
    }
}