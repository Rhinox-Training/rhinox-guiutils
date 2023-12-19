using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GenericPropertyMemberHelper<T> : BaseValueMemberHelper<T>, IPropertyMemberHelper<T>
    {
        public GenericPropertyMemberHelper(object property, string input, ref string errorMessage)
            : this(property, input)
        {
            if (errorMessage != null)
                return;
            errorMessage = this.ErrorMessage;
        }
        
        public GenericPropertyMemberHelper(object property, string input)
            : this(property?.GetType(), input, property)
        {
        }
        
        public GenericPropertyMemberHelper(Type type, string input)
            : this(type, input, null)
        {
        }

        private GenericPropertyMemberHelper(Type type, string input, object host)
        {
            if (host is GenericHostInfo hostInfo)
            {
                _hostInfo = hostInfo;
                _host = FetchInstanceFrom(_hostInfo);
                _objectType = _host?.GetType();
            }
            else
            {
                _objectType = type;
                _host = host;
            }
            
            if (!TryParseInput(ref input, out bool parameter))
                return;

            if (!parameter && typeof(T) == typeof(string))
            {
                _cachedValue = (T) (object) input;
                return;
            }

            // target might have changed
            if (_host != null)
                _objectType = _host?.GetType();

            if (!TryFindMemberInHost(input, _host == null, out _staticValueGetter, out _instanceValueGetter))
                _errorMessage = $"Could not find field {input} on type {_objectType.Name}";
        }

        protected override object GetInstance()
        {
            if (_host != null)
                return _host;
            
            if (_hostInfo != null)
                _host = FetchInstanceFrom(_hostInfo);
            
            return _host;
        }

        private static object FetchInstanceFrom(GenericHostInfo info)
        {
            if (info == null) return null;
            
            if (info.ArrayIndex >= 0)
                return info.GetValue();
            return info.GetHost();
        }
    }
}