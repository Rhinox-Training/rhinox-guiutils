using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    public static class SerializedObjectExtensions
    {
        public static Type GetParentType(this SerializedProperty prop)
        {
            return prop.serializedObject?.targetObject.GetType();
        }
    }
    
    /// <summary>
    /// VERY BASIC VERSION FOR WHEN NO ACCESS TO ODIN
    /// Helper class to handle strings for labels and other similar purposes.
    /// Allows for a static string, or for referring to string member fields, properties or methods, by name
    /// </summary>
    public class PropertyMemberHelper<T>
    {
        private T _cachedValue;
        private string _errorMessage;
        private readonly Type _objectType;
        
        private Func<T> _staticValueGetter;
        private Func<object, T> _instanceValueGetter;
        
        private UnityEngine.Object _propertyHost;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        public PropertyMemberHelper(SerializedProperty property, string text)
            : this(property.serializedObject == null, text, property)
        {
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        public PropertyMemberHelper(SerializedProperty property, string text, ref string errorMessage)
            : this(property, text)
        {
            if (errorMessage != null)
                return;
            errorMessage = this.ErrorMessage;
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="objectType">The type of the parent, to get a member string from.</param>
        /// <param name="isStatic">Value indicating if the context should be static.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        private PropertyMemberHelper(bool isStatic, string text, SerializedProperty property)
        {
            _propertyHost = property.serializedObject.targetObject;
            _objectType = property.GetParentType();
            
            if (string.IsNullOrEmpty(text) || _objectType == null || text.Length <= 0)
                return;
            
            if (text[0] == '@')
            {
                this._errorMessage = "Expressions are only supported with Odin Enabled";
                return;
            }

            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";

            if (text[0] == '$')
            {
                text = text.Substring(1);
                int partI = -1;
                while ((partI = text.IndexOf(".", StringComparison.Ordinal)) >= 0)
                {
                    var part = text.Substring(0, partI);
                    bool actionTaken = true;
                    switch (part)
                    {
                        case PARENT_ID:
                            _errorMessage = $"${part} is only supported with Odin Enabled";
                            break;
                        case ROOT_ID:
                            _errorMessage = $"${part} is only supported with Odin Enabled";
                            break;
                        default:
                            actionTaken = false;
                            break;
                    }

                    if (!actionTaken)
                        break;
                }
            }
            
            // property might have changed
            _objectType = property.GetParentType();

            var flags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = _objectType.FindMembers(
                    MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    flags, 
                    (mi, crit) => mi.GetReturnType() == typeof(T), 
                    null
            );

            var member = members.FirstOrDefault();

            switch (member)
            {
                case MethodInfo mi:
                    if (mi.IsStatic())
                        this._staticValueGetter = () => (T) mi.Invoke(null, new object[] {});
                    else
                        this._instanceValueGetter = (i) => (T) mi.Invoke(i, new object[] {});
                    break;
                case FieldInfo fi:
                    if (fi.IsStatic())
                        this._staticValueGetter = () => (T) fi.GetValue(null);
                    else
                        this._instanceValueGetter = (i) => (T) fi.GetValue(i);
                    break;
                case PropertyInfo pi:
                    if (pi.IsStatic())
                        this._staticValueGetter = () => (T) pi.GetValue(null);
                    else
                        this._instanceValueGetter = (i) => (T) pi.GetValue(i);
                    break;
                default:
                    break;
            }

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
            if (this._cachedValue == null || Event.current == null || Event.current.type == EventType.Layout)
                this._cachedValue = this.ForceGetValue(_propertyHost);
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