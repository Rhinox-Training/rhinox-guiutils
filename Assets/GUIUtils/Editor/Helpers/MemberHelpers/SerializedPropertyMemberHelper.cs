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
    public class SerializedPropertyMemberHelper<T> : BaseValueMemberHelper<T>, IPropertyMemberHelper<T>
    {
        private HostInfo _info;

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
            
            _info = property.GetHostInfo();
            
            if (!TryParseInput(ref input))
                return;
            
            // property might have changed
            _objectType = _info.GetHostType();
            
            var members = FindMembers(isStatic, (info, _) => info.GetReturnType().InheritsFrom(typeof(T)) && info.Name == input);

            var mi = members.FirstOrDefault();
            
            if (mi == null)
                _errorMessage = $"Could not find field {input} on type {_objectType.Name}";
            else if (mi.IsStatic())
                this._staticValueGetter = () => (T) mi.GetValue(null);
            else
                this._instanceValueGetter = (i) => (T) mi.GetValue(i);
        }
        
        protected override bool TryParseParameter(string input)
        {
            if (!base.TryParseParameter(input))
                return false;
            
            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";
            
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
            
            return true;
        }

        protected override object GetInstance() => _info?.GetHost();
    }
}