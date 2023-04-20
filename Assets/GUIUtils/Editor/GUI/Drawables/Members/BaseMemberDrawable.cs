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
    public abstract class BaseMemberDrawable<T> : BaseDrawable, IDrawableReadWrite
    {
        public override string LabelString => HostInfo.NiceName;
        
        public GenericHostInfo HostInfo { get; }

        protected BaseMemberDrawable(object instance, MemberInfo info)
            : this(new GenericHostInfo(instance, info))
        {
        }
        
        protected BaseMemberDrawable(GenericHostInfo hostInfo)
        {
            HostInfo = hostInfo;
            Host = hostInfo.Parent ?? hostInfo.GetHost();
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            var smartVal = GetSmartValue();
            var newVal = DrawValue(label, smartVal, options);
            PostProcessValue(ref newVal);
            if (!object.Equals(newVal, smartVal))
                SetSmartValue(newVal);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var smartVal = GetSmartValue();
            var newVal = DrawValue(rect, label, smartVal);
            PostProcessValue(ref newVal);
            if (!object.Equals(newVal, smartVal))
                SetSmartValue(newVal);
        }

        protected virtual void PostProcessValue(ref T value)
        {
        }

        public object GetValue() => HostInfo.GetValue();
        public bool TrySetValue(object value) => HostInfo.TrySetValue(value);

        protected T GetSmartValue() => HostInfo.GetSmartValue<T>();
        protected bool SetSmartValue(T value) => HostInfo.TrySetValue(value);

        protected abstract T DrawValue(GUIContent label, T value, params GUILayoutOption[] options);
        protected abstract T DrawValue(Rect rect, GUIContent label, T value);

        private Attribute[] _cachedAttributes;
        
        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_cachedAttributes == null)
                _cachedAttributes = HostInfo.GetAttributes();
            
            return _cachedAttributes.OfType<TAttribute>();
        }
    }
}