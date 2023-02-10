using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseSmartDrawableMember<T> : IDrawableMember
    {
        protected MemberInfo _info;
        
        public BaseSmartDrawableMember(MemberInfo info)
        {
            _info = info;
        }
        
        public object Draw(object target) => DrawWithSmartValue(target);

        public T GetSmartValue(object target) => (T) _info.GetValue(target);

        public abstract T DrawWithSmartValue(object target);
    }
}