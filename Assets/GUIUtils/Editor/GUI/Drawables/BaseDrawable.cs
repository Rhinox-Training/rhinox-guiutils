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
        
        private GUIContent _cachedLabel;
        public virtual GUIContent Label {
            get
            {
                if (_cachedLabel == null)
                    _cachedLabel = string.IsNullOrEmpty(LabelString) ? GUIContent.none : new GUIContent(LabelString);
                return _cachedLabel; 
            }
        }

        public abstract string LabelString { get; }
        
        public object Host { get; protected set; }
        public virtual bool IsVisible => true;

        private bool _initialized;

        protected BaseDrawable()
        {
            _initialized = false;
        }

        public void Draw(GUIContent label, params GUILayoutOption[] options)
        {
            OnPreDraw();
            
            DrawInner(label, options);
            
            OnPostDraw();
        }

        public void Draw(Rect rect, GUIContent label)
        {
            OnPreDraw();
            
            DrawInner(rect, label);
            
            OnPostDraw();
        }
        
        protected virtual void DrawInner(GUIContent label, params GUILayoutOption[] options)
            => DrawInner(label);

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
        
        public TAttribute GetDrawableAttribute<TAttribute>() 
            where TAttribute : Attribute
        {
            return GetDrawableAttributes<TAttribute>().FirstOrDefault();
        }
        
        public bool TryGetDrawableAttribute<TAttribute>(out TAttribute attribute) 
            where TAttribute : Attribute
        {
            var attributes = GetDrawableAttributes<TAttribute>();
            attribute = attributes.FirstOrDefault();
            return !attributes.IsNullOrEmpty();
        }
        
        protected virtual void Initialize()
        {
            var orderAttr = GetDrawableAttributes<PropertyOrderAttribute>().FirstOrDefault();
            if (orderAttr != null)
                Order = orderAttr.Order;
        }
    }
}