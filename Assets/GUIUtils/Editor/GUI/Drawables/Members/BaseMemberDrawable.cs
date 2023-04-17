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
    public abstract class BaseMemberDrawable<T> : BaseDrawable, IMemberDrawable
    {
        public override string LabelString => Entry.NiceName;
        
        public GenericMemberEntry Entry { get; }

        protected BaseMemberDrawable(object instance, MemberInfo info)
            : this(new GenericMemberEntry(instance, info))
        {
        }
        
        protected BaseMemberDrawable(GenericMemberEntry entry)
        {
            Entry = entry;
            Host = entry;
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

        protected T GetSmartValue() => Entry.GetSmartValue<T>();
        protected bool SetSmartValue(T value) => Entry.TrySetValue(value);

        protected abstract T DrawValue(GUIContent label, T value, params GUILayoutOption[] options);
        protected abstract T DrawValue(Rect rect, GUIContent label, T value);

        private Attribute[] _cachedAttributes;
        
        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_cachedAttributes == null)
                _cachedAttributes = Entry.GetAttributes();
            
            return _cachedAttributes.OfType<TAttribute>();
        }
    }
}