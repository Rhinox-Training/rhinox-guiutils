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
    /// Support special cases: $root, $parent, $property
    /// </summary>
    public class SerializedPropertyMemberHelper<T> : BaseValueMemberHelper<T>, IPropertyMemberHelper<T>
    {
        private HostInfo _info;

        public SerializedPropertyMemberHelper(SerializedProperty property, string input)
            : this(property.serializedObject == null, input, property)
        {
        }

        public SerializedPropertyMemberHelper(SerializedProperty property, string input, ref string errorMessage)
            : this(property, input)
        {
            if (errorMessage != null)
                return;
            errorMessage = this.ErrorMessage;
        }
        
        private SerializedPropertyMemberHelper(bool isStatic, string input, SerializedProperty property)
        {
            _objectType = property.GetHostType();
            
            _info = property.GetHostInfo();
            
            if (!TryParseInput(ref input, out bool parameter))
                return;

            if (!parameter && typeof(T) == typeof(string))
            {
                _cachedValue = (T) (object) input;
                return;
            }
            
            // property might have changed
            _objectType = _info.GetHostType();
            
            if (!TryFindMemberInHost(input, isStatic, out _staticValueGetter, out _instanceValueGetter))
                _errorMessage = $"Could not find field {input} on type {_objectType.Name}";
        }
        
        protected override bool TryParseParameter(ref string input)
        {
            if (!base.TryParseParameter(ref input))
                return false;
            
            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";
            const string PROPERTY_ID = "property";

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
                    case PROPERTY_ID:
                        break;
                    default:
                        actionTaken = false;
                        break;
                }

                if (!actionTaken)
                    break;
                
                input = input.Substring(part.Length + 1);
            }
            
            return true;
        }

        protected override object GetInstance() => _info?.GetHost();
    }
}