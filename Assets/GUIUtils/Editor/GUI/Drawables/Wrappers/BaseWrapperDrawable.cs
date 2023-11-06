using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseWrapperDrawable : BaseDrawable
    {
        protected static class Priority
        {
            /// <summary>
            /// Changes default editor GUI
            /// </summary>
            public const int Simple = -1;
            /// <summary>
            /// Changes default editor GUI, taking priority over Simple
            /// </summary>
            public const int Important = -10;
            /// <summary>
            /// Will pass through, but adds something
            /// </summary>
            public const int Append = -500;
            /// <summary>
            /// Will pass through, but changes some variables / GUI state
            /// </summary>
            public const int BehaviourChange = -1000;
            /// <summary>
            /// Will stop certain things from happening
            /// </summary>
            public const int BehaviourPrevention = -1250;
        }
        
        protected IOrderedDrawable _innerDrawable;
        
        public override float ElementHeight => _innerDrawable.ElementHeight;

        public override bool IsVisible => _innerDrawable.IsVisible;

        public override GUIContent Label => _innerDrawable.Label;

        protected override string LabelString => string.Empty; // Not used in wrapper

        protected BaseWrapperDrawable(IOrderedDrawable drawable)
        {
            if (drawable == null) throw new ArgumentNullException(nameof(drawable));
            _innerDrawable = drawable;
            _hostInfo = _innerDrawable.HostInfo;
            Order = _innerDrawable.Order;

            _innerDrawable.RepaintRequested += RequestRepaint;
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

        protected object GetValue()
        {
            if (_innerDrawable is IDrawableRead memberDrawable)
                return memberDrawable.GetValue();

            return null;
        }
        
        protected bool SetValue(object value)
        {
            if (_innerDrawable is IDrawableReadWrite memberDrawable)
                return memberDrawable.TrySetValue(value);
            
            Debug.LogError("Could not set value....");
            return false;
        }
    }
}