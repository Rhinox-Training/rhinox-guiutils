using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseEntityDrawable<T> : BaseEntityDrawable
    {
        protected T Entity => (T) EntityInstance;
        
        protected BaseEntityDrawable(T instanceVal, MemberInfo memberInfo = null) : base(instanceVal, memberInfo)
        {
        }
    }
    
    public abstract class BaseEntityDrawable : BaseDrawable
    {
        protected object EntityInstance { get; }
        protected readonly MemberInfo _memberInfo;

        public override string LabelString => Host?.GetType().Name;

        public override ICollection<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_memberInfo != null)
            {
                return _memberInfo.GetCustomAttributes<TAttribute>().ToArray();
            }
            return base.GetDrawableAttributes<TAttribute>();
        }

        protected BaseEntityDrawable(object instanceVal, MemberInfo memberInfo = null)
        {
            Host = null;
            EntityInstance = instanceVal;
            _memberInfo = memberInfo;
        }
    }
}