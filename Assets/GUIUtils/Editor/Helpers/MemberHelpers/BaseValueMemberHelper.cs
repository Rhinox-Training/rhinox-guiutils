using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseValueMemberHelper<T> : BaseMemberHelper
    {
        protected T _cachedValue;

        protected Func<T> _staticValueGetter;
        protected Func<object, T> _instanceValueGetter;
        
        public bool IsDynamicString => this._instanceValueGetter != null
                                       || this._staticValueGetter != null;

        protected abstract object GetInstance();

        public object GetValue()
        {
            return GetSmartValue();
        }
        
        public T GetSmartValue()
        {
            if (_newFrameHandler.IsNewFrame())
            {
                this._cachedValue = this.ForceGetValue(GetInstance());
            }
            return this._cachedValue;
        }

        public T GetValue(ref bool changed)
        {
            var oldValue = _cachedValue;
            var newValue = GetSmartValue();
            if (!Equals(oldValue, newValue))
                changed = true;
            
            return newValue;
        }

        public T ForceGetValue()
        {
            return ForceGetValue(GetInstance());
        }

        /// <summary>Forcefully fetches a new value, ignoring any caches.</summary>
        public T ForceGetValue(object instance)
        {
            if (this._errorMessage != null)
                return default;

            if (this._staticValueGetter != null)
                return this._staticValueGetter();

            if (_instanceValueGetter != null)
                return _instanceValueGetter(instance);
            
            return _cachedValue;
        }
    }
}