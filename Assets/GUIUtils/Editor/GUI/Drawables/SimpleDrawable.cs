using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class SimpleDrawable : IOrderedDrawable
    {
        public Color? Colour { get; set; }
        public float Order { get; set; }

        public virtual float ElementHeight => EditorGUIUtility.singleLineHeight;

        private readonly object _targetObj;

        protected SimpleDrawable(object obj, int order = 0)
        {
            _targetObj = obj;
            Order = order;
        }

        public void Draw(Rect rect)
        {
            DrawInternal(rect);
        }

        public virtual ICollection<TAttribute> GetMemberAttributes<TAttribute>() where TAttribute : Attribute
        {
            return Array.Empty<TAttribute>();
        }

        public void Draw()
        {
            DrawInternal(null);
        }

        private void DrawInternal(Rect? rect)
        {
            if (Colour != null)
            {
                var color = Colour.Value;
                using (new eUtility.GuiBackgroundColor(color))
                {
                    DrawForTargets(rect);
                }
            }
            else
            {
                DrawForTargets(rect);
            }
        }

        private void DrawForTargets(Rect? r = null)
        {
            if (r.HasValue)
                Draw(r.Value, _targetObj);
            else
                Draw(_targetObj);
        }

        protected abstract void Draw(object target);
        protected abstract void Draw(Rect rect, object target);
    }
}