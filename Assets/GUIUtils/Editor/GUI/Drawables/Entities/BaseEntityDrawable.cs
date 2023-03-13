using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseEntityDrawable : BaseDrawable
    {
        protected readonly object _targetObj;
        private readonly MemberInfo _memberInfo;

        protected override string LabelString => _targetObj != null ? _targetObj.GetType().Name : null;

        public override ICollection<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_memberInfo != null)
            {
                return _memberInfo.GetCustomAttributes<TAttribute>().ToArray();
            }
            return base.GetDrawableAttributes<TAttribute>();
        }

        protected BaseEntityDrawable(object obj)
        {
            _targetObj = obj;
            _memberInfo = null;
        }
        
        protected BaseEntityDrawable(object obj, MemberInfo memberInfo)
        {
            _targetObj = obj;
            _memberInfo = memberInfo;
        }
        
        protected override void DrawInner(GUIContent label)
        {
            Draw(_targetObj);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            Draw(rect, _targetObj);
        }
        
        protected abstract void Draw(object target);
        protected abstract void Draw(Rect rect, object target);
    }
}