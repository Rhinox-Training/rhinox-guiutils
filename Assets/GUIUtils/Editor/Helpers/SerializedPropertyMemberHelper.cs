using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// VERY BASIC VERSION OF PropertyMemberHelper FOR WHEN NO ACCESS TO ODIN
    /// Helper class to handle strings for labels and other similar purposes.
    /// Allows for a static string, or for referring to string member fields, properties or methods, by name
    /// </summary>
    public class SerializedPropertyMemberHelper<T> : IPropertyMemberHelper<T>
    {
        private T _cachedValue;
        private string _errorMessage;
        private readonly Type _objectType;

        private Func<T> _staticValueGetter;
        private Func<object, T> _instanceValueGetter;
        
        private HostInfo _info;
        private NewFrameHandler _newFrameHandler;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="input">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        public SerializedPropertyMemberHelper(SerializedProperty property, string input)
            : this(property.serializedObject == null, input, property)
        {
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="input">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="input">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        public SerializedPropertyMemberHelper(SerializedProperty property, string input, ref string errorMessage)
            : this(property, input)
        {
            if (errorMessage != null)
                return;
            errorMessage = this.ErrorMessage;
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="objectType">The type of the parent, to get a member string from.</param>
        /// <param name="isStatic">Value indicating if the context should be static.</param>
        /// <param name="input">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="input">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        private SerializedPropertyMemberHelper(bool isStatic, string input, SerializedProperty property)
        {
            _objectType = property.GetHostType();
            
            if (string.IsNullOrEmpty(input) || _objectType == null || input.Length <= 0)
                return;
            
            if (input[0] == '@')
            {
                this._errorMessage = "Expressions are only supported with Odin Enabled";
                return;
            }
            
            _info = property.GetHostInfo();

            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";

            if (input[0] == '$')
            {
                input = input.Substring(1);
                int partI = -1;
                while ((partI = input.IndexOf(".", StringComparison.Ordinal)) >= 0)
                {
                    var part = input.Substring(0, partI);
                    bool actionTaken = true;
                    switch (part)
                    {
                        case PARENT_ID:
                            _info = _info.Parent;
                            break;
                        case ROOT_ID:
                            while (_info.Parent != null)
                                _info = _info.Parent;
                            break;
                        default:
                            actionTaken = false;
                            break;
                    }

                    if (actionTaken)
                        input = input.Substring(part.Length+1);
                    else
                        break;
                }
            }
            
            // property might have changed
            _objectType = _info.GetHostType();

            var flags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = _objectType.FindMembers(
                    MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    flags, 
                    (info, crit) => info.GetReturnType() == typeof(T) && info.Name == input, 
                    null
            );

            var mi = members.FirstOrDefault();
            
            if (mi == null)
                _errorMessage = $"Could not find field {input} on type {_objectType.Name}";
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
        /// </summary>
        public T GetValue()
        {
            if (_newFrameHandler.IsNewFrame())
            {
                this._cachedValue = this.ForceGetValue(_info?.GetHost());
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

        public T ForceGetValue()
        {
            var instance = _info?.GetHost();
            return ForceGetValue(instance);
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
            
            return default;
        }
    }
}