using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseEntityDrawable<T> : BaseEntityDrawable
    {
        public T Entity => (T) Instance;

        public override Type ObjectType => typeof(T);

        protected BaseEntityDrawable(T instanceVal, MemberInfo memberInfo = null) 
            : base(instanceVal, memberInfo)
        {
        }
    }
    
    public abstract class BaseEntityDrawable : BaseDrawable, IObjectDrawable
    {
        public object Instance { get; }

        public virtual Type ObjectType
        {
            get
            {
                if (Instance != null)
                    return Instance.GetType();
                if (_memberInfo != null)
                    _memberInfo.GetReturnType();
                return typeof(System.Object);
            }
        }
        protected readonly MemberInfo _memberInfo;

        public override string LabelString => Host?.GetType().Name;

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_memberInfo == null)
                return base.GetDrawableAttributes<TAttribute>();
            return _memberInfo.GetCustomAttributes<TAttribute>();
        }

        protected BaseEntityDrawable(object instanceVal, MemberInfo memberInfo = null)
        {
            Host = null;
            Instance = instanceVal;
            _memberInfo = memberInfo;
        }
    }
}