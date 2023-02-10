using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseDrawable<T> : IOrderedDrawable
    {
        public int Order { get; set; }
        
        protected MemberInfo _info;
        private object _instance;

        public BaseDrawable(object instance, MemberInfo info)
        {
            _instance = instance;
            _info = info;
        }
        
        protected T GetSmartValue() => (T) _info.GetValue(_instance);
        protected void SetSmartValue(T val) => _info.SetValue(_instance, val);
        
        public void Draw()
        {
            var smartVal = GetSmartValue();
            var newVal = DrawValue(_instance, smartVal);
            SetSmartValue(newVal);
        }

        public void Draw(Rect rect)
        {
            var smartVal = GetSmartValue();
            var newVal = DrawValue(rect, _instance, smartVal);
            SetSmartValue(newVal);
        }

        protected abstract T DrawValue(object instance, T memberVal);
        protected abstract T DrawValue(Rect rect, object instance, T memberVal);
    }
}