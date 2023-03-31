using System;
using System.Linq;
using Rhinox.Lightspeed.Reflection;

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
            : this(property == null, property?.GetType(), input)
        {
            _host = property;
        }
        
        public GenericPropertyMemberHelper(Type type, string input)
            : this(true, type, input)
        {
            _host = null;
        }

        private GenericPropertyMemberHelper(bool isStatic, Type type, string input)
        {
            _objectType = type;

            if (!TryParseInput(ref input, out bool parameter))
                return;
            
            if (!parameter && typeof(T) == typeof(string))
            {
                _cachedValue = (T) (object) input;
                return;
            }

            var members = FindMembers(isStatic, (info, _) => info.GetReturnType().InheritsFrom(typeof(T)) && info.Name == input);

            var mi = members.FirstOrDefault();
            
            if (mi == null)
                _errorMessage = $"Could not find field {input} on type {_objectType.Name}";
            else if (mi.IsStatic())
                _staticValueGetter = () => (T) mi.GetValue(null);
            else
                _instanceValueGetter = (i) => (T) mi.GetValue(i);
        }

        protected override object GetInstance() => _host;
    }
}