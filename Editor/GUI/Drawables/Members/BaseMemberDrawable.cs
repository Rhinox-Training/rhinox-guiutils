using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseMemberDrawable<T> : BaseDrawable
    {
        protected override string LabelString => _info != null ? _info.Name : null;

        public override ICollection<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_info == null)
                return base.GetDrawableAttributes<TAttribute>();
            return _info.GetCustomAttributes<TAttribute>().ToArray();
        }
        
        protected MemberInfo _info;
        private object _instance;

        public BaseMemberDrawable(object instance, MemberInfo info)
        {
            _instance = instance;
            _info = info;
        }

        protected override void Initialize()
        {
            base.Initialize();
            IsReadOnly = AttributeParser.ParseReadOnly(_info);
        }

        protected override void DrawInner(GUIContent label)
        {
            var smartVal = GetSmartValue();
            var newVal = DrawValue(_instance, smartVal);
            if (!IsReadOnly)
                SetSmartValue(newVal);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var smartVal = GetSmartValue();
            var newVal = DrawValue(rect, _instance, smartVal);
            if (!IsReadOnly)
                SetSmartValue(newVal);
        }

        protected T GetSmartValue() => (T) _info.GetValue(_instance);
        protected void SetSmartValue(T val) => _info.SetValue(_instance, val);

        protected abstract T DrawValue(object instance, T memberVal);
        protected abstract T DrawValue(Rect rect, object instance, T memberVal);
    }
}