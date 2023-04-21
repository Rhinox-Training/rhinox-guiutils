using System.Reflection;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseMemberValueDrawable<T> : BaseMemberDrawable, IDrawableReadWrite
    {

        protected BaseMemberValueDrawable(object instance, MemberInfo info)
            : this(new GenericHostInfo(instance, info))
        {
        }
        
        protected BaseMemberValueDrawable(GenericHostInfo hostInfo)
            :base(hostInfo)
        {
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
    }
}