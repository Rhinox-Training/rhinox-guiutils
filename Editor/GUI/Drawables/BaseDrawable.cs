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
        
        public GUIContent Label => HideLabel || string.IsNullOrEmpty(LabelString) ? GUIContent.none : GUIContentHelper.TempContent(LabelString);
        
        public abstract string LabelString { get; }
        
        public object Host { get; protected set; }
        
        public virtual ICollection<TAttribute> GetDrawableAttributes<TAttribute>() 
            where TAttribute : Attribute
        {
            return Array.Empty<TAttribute>();
        }
        
        private bool _initialized;

        protected BaseDrawable()
        {
            _initialized = false;
        }

        public void Draw()
        {
            OnPreDraw();
            
            DrawInner(Label);
            
            OnPostDraw();
        }

        public void Draw(Rect rect)
        {
            OnPreDraw();
            
            DrawInner(rect, Label);
            
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

        protected virtual void Initialize()
        {
            var orderAttr = GetDrawableAttributes<PropertyOrderAttribute>().FirstOrDefault();
            if (orderAttr != null)
                Order = orderAttr.Order;

            HideLabel = !GetDrawableAttributes<HideLabelAttribute>().IsNullOrEmpty();
        }
    }
}