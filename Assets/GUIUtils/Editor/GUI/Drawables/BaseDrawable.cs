using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseDrawable : IOrderedDrawable
    {
        public float Order { get; set; }
        public virtual float ElementHeight => EditorGUIUtility.singleLineHeight;
        
        public bool HideLabel { get; protected set; }
        
        public virtual GUIContent Label => HideLabel || string.IsNullOrEmpty(LabelString) ? null : GUIContentHelper.TempContent(LabelString);
        
        public abstract string LabelString { get; }
        
        public object Host { get; protected set; }
        public virtual bool IsVisible => true;

        private bool _initialized;

        protected BaseDrawable()
        {
            _initialized = false;
        }

        public void Draw(GUIContent label)
        {
            OnPreDraw();
            
            DrawInner(label);
            
            OnPostDraw();
        }

        public void Draw(Rect rect, GUIContent label)
        {
            OnPreDraw();
            
            DrawInner(rect, label);
            
            OnPostDraw();
        }
        
        protected abstract void DrawInner(GUIContent label);
        protected abstract void DrawInner(Rect rect, GUIContent label);

        /// <summary>
        /// Gets called predraw to refresh fields
        /// </summary>
        protected virtual void OnPreDraw()
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }
        }
        
        protected virtual void OnPostDraw()
        {
            
        }
        
        public virtual ICollection<TAttribute> GetDrawableAttributes<TAttribute>() 
            where TAttribute : Attribute
        {
            return Array.Empty<TAttribute>();
        }
        
        protected virtual void Initialize()
        {
            var orderAttr = GetDrawableAttributes<PropertyOrderAttribute>().FirstOrDefault();
            if (orderAttr != null)
                Order = orderAttr.Order;

            HideLabel = !GetDrawableAttributes<HideLabelAttribute>().IsNullOrEmpty();
        }
    }
}