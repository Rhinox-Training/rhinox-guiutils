using Sirenix.Utilities;
using Sirenix.Utilities.Editor.Expressions;
using System;
using System.Collections;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    /// <summary>
    /// Helper class to handle strings for labels and other similar purposes.
    /// Allows for a static string, or for referring to string member fields, properties or methods, by name
    /// </summary>
    public class PropertyMemberHelper<T>
    {
        private T _cachedValue;
        private string _errorMessage;
        private readonly Type _objectType;
        
        private bool _isStatic;
        
        private Func<T> _staticValueGetter;
        private Func<object, T> _instanceValueGetter;
        
        private InspectorProperty _property;
        private Delegate _expressionMethod;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage
        {
            get { return this._errorMessage; }
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        public PropertyMemberHelper(InspectorProperty property, string text)
            : this(property.ParentValueProperty == null && property.Tree.IsStatic, text, property)
        {
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        public PropertyMemberHelper(InspectorProperty property, string text, ref string errorMessage)
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
        private PropertyMemberHelper(bool isStatic, string text, InspectorProperty property)
        {
            _property = property;
            _objectType = property.ParentType;
            _isStatic = isStatic;
            
            if (string.IsNullOrEmpty(text) || _objectType == null || text.Length <= 0)
                return;
            
            if (text[0] == '@')
            {
                Type[] parameters = null;
                string[] parameterNames = null;
                if (_property != null)
                {
                    parameters = new System.Type[1]
                    {
                        typeof(InspectorProperty)
                    };
                    parameterNames = new string[1]
                    {
                        property.Name
                    };
                }

                this._expressionMethod = ExpressionUtility.ParseExpression(text.Substring(1), this._isStatic, _objectType, parameters,
                    parameterNames,
                    out this._errorMessage, true);

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
                            text = text.Substring(partI + 1); // + 1 to include the .
                            var previousType = this._property.ParentType;
                            this._property = this._property.ParentValueProperty;
                            var newType = this._property.ParentType;
                            
                            // Minor hack to go 1 deeper when working with arrays
                            if (newType.IsArray && newType.GetElementType() == previousType)
                                this._property = this._property.ParentValueProperty;

                            break;
                        case ROOT_ID:
                            text = text.Substring(partI + 1); // + 1 to include the .
                            this._property = this._property.SerializationRoot;
                            break;
                        default:
                            actionTaken = false;
                            break;
                    }

                    if (!actionTaken)
                        break;
                }
            }
            
            this._objectType = this._property.ParentType;

            MemberFinder memberFinder = MemberFinder.Start(_objectType).HasReturnType<T>(true).IsNamed(text).HasNoParameters();
            if (isStatic)
                memberFinder = memberFinder.IsStatic();
            MemberInfo memberInfo;
            if (!memberFinder.TryGetMember(out memberInfo, out this._errorMessage))
                return;
            if (memberInfo is MethodInfo)
                text += "()";
            if (memberInfo.IsStatic())
                this._staticValueGetter = DeepReflection.CreateValueGetter<T>(_objectType, text, true);
            else
                this._instanceValueGetter = DeepReflection.CreateWeakInstanceValueGetter<T>(_objectType, text, true);
        }

        /// <summary>
        /// Gets a value indicating whether or not the string is retrieved from a from a member.
        /// </summary>
        public bool IsDynamicString => this._expressionMethod != null
                                       || this._instanceValueGetter != null
                                       || this._staticValueGetter != null;

        /// <summary>Gets the type of the object.</summary>
        public Type ObjectType => this._objectType;
        
        /// <summary>
        /// Gets the string from the StringMemberHelper.
        /// Only updates the string buffer in Layout events.
        /// </summary>
        /// <param name="instance">The instance, for evt. member references.</param>
        /// <returns>The current string.</returns>
        public T GetValue()
        {
            var instance = _property.ParentValues[0];
            if (this._cachedValue == null || Event.current == null || Event.current.type == EventType.Layout)
                this._cachedValue = this.ForceGetValue(instance);
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

        /// <summary>Gets the string from the StringMemberHelper.</summary>
        /// <param name="entry">The property entry, to get the instance reference from.</param>
        /// <returns>The current string.</returns>
        public T ForceGetValue(IPropertyValueEntry entry)
        {
            return this.ForceGetValue(entry.Property.ParentValues[0]);
        }

        /// <summary>Gets the string from the StringMemberHelper.</summary>
        /// <param name="property">The property, to get the instance reference from.</param>
        /// <returns>The current string.</returns>
        public T ForceGetValue(InspectorProperty property)
        {
            return this.ForceGetValue(property.ParentValues[0]);
        }

        /// <summary>Gets the string from the StringMemberHelper.</summary>
        /// <param name="instance">The instance, for evt. member references.</param>
        /// <returns>The current string.</returns>
        public T ForceGetValue(object instance)
        {
            if (this._errorMessage != null)
                return default;
            if ((object) this._expressionMethod != null)
            {
                T obj;
                if (this._isStatic)
                {
                    if (this._property != null)
                        obj = (T) this._expressionMethod.DynamicInvoke((object) this._property);
                    else
                        obj = (T) this._expressionMethod.DynamicInvoke();
                }
                else if (this._property != null)
                    obj = (T) this._expressionMethod.DynamicInvoke(instance, (object) this._property);
                else
                    obj = (T) this._expressionMethod.DynamicInvoke(instance);

                if (obj != null)
                    return obj;
                return default;
            }

            if (this._staticValueGetter != null)
                return this._staticValueGetter();

            if (_instanceValueGetter != null)
                return _instanceValueGetter(instance);
            
            return default;
        }
    }
}