using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GenericPropertyMemberHelper<T> : IPropertyMemberHelper<T>
    {
        private T _cachedValue;
        private string _errorMessage;
        private readonly Type _objectType;

        private Func<T> _staticValueGetter;
        private Func<object, T> _instanceValueGetter;
        
        private object _host;
        private NewFrameHandler _newFrameHandler;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        public GenericPropertyMemberHelper(object property, string text, ref string errorMessage)
            : this(property, text)
        {
            if (errorMessage != null)
                return;
            errorMessage = this.ErrorMessage;
        }
        
        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        public GenericPropertyMemberHelper(object property, string text)
            : this(property == null, property?.GetType(), text)
        {
            _host = property;
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="objectType">The type of the parent, to get a member string from.</param>
        /// <param name="isStatic">Value indicating if the context should be static.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        private GenericPropertyMemberHelper(bool isStatic, Type type, string text)
        {
            _objectType = type;
            
            if (string.IsNullOrEmpty(text) || _objectType == null || text.Length <= 0)
                return;
            
            if (text[0] == '@')
            {
                this._errorMessage = "Expressions are only supported with Odin Enabled";
                return;
            }

            if (text[0] == '$')
                text = text.Substring(1);

            var flags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = _objectType.FindMembers(
                    MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    flags, 
                    (info, crit) => info.GetReturnType().InheritsFrom(typeof(T)) && info.Name == text, 
                    null
            );

            var mi = members.FirstOrDefault();
            
            if (mi == null)
                _errorMessage = $"Could not find field {text} on type {_objectType.Name}";
            else if (mi.IsStatic())
                this._staticValueGetter = () => (T) mi.GetValue(null);
            else
                this._instanceValueGetter = (i) => (T) mi.GetValue(i);
        }

       

        /// <summary>
        /// Gets a value indicating whether or not the string is retrieved from a from a member.
        /// </summary>
        public bool IsDynamicString => this._instanceValueGetter != null
                                       || this._staticValueGetter != null;

        /// <summary>Gets the type of the object.</summary>
        public Type ObjectType => this._objectType;
        
        /// <summary>
        /// Gets the value from the MemberHelper.
        /// This caches the value during the same frame, so it can be used efficiently in draw code
        /// This also means you aren't guaranteed to have an up-to-date value
        /// </summary>
        public T GetValue()
        {
            if (_newFrameHandler.IsNewFrame())
            {
                this._cachedValue = this.ForceGetValue();
            }
            return this._cachedValue;
        }

        public T GetValue(ref bool changed)
        {
            var oldValue = _cachedValue;
            var newValue = GetValue();
            if (!Equals(oldValue, newValue))
                changed = true;
            
            return newValue;
        }

        /// <summary>
        /// Fetches an up-to-date value
        /// Inefficient when called in standard draw code (layout, repaint, etc = multiple times per frame)
        /// </summary>
        public T ForceGetValue()
        {
            if (this._errorMessage != null)
                return default;

            if (this._staticValueGetter != null)
                return this._staticValueGetter();

            if (_instanceValueGetter != null)
                return _instanceValueGetter(_host);
            
            return default;
        }

        
    }
}