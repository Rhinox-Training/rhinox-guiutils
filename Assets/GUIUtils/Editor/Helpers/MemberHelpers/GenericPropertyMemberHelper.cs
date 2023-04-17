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
        private GenericMemberEntry _entry;
        
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
            if (host is GenericMemberEntry entry)
            {
                _entry = entry;
                _host = _entry.Instance;
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

        protected override bool TryParseParameter(ref string input)
        {
            if (!base.TryParseParameter(ref input))
                return false;

            if (input.IsNullOrEmpty())
                return false;
            
            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";
            const string PROPERTY_ID = "property";
            const string VALUE_ID = "value";

            GenericMemberEntry usedEntry = null;
            string[] parts = input.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; ++i)
            {
                bool actionTaken = true;
                
                switch (parts[i])
                {
                    case PROPERTY_ID:
                        if (_entry != null)
                        {
                            _host = _entry;
                            _objectType = _host?.GetType();
                        }
                        break;
                    case PARENT_ID:
                        if (_entry != null)
                            usedEntry = _entry.Parent;
                        break;
                    case ROOT_ID:
                        if (usedEntry != null)
                        {
                            usedEntry = _entry;

                            while (usedEntry.Parent != null)
                                usedEntry = usedEntry.Parent;
                        }
                        break;
                    case VALUE_ID:
                        if (_entry != null)
                        {
                            _host = _entry.GetValue();
                        }
                        break;
                    default:
                        if (i != parts.Length - 1) // if we're not at the last part
                        {
                            // try to resolve it
                            if (TryFindMemberInHost(parts[i], null, false, out MemberInfo info))
                            {
                                _host = info.GetValue(_host);
                                _objectType = info.GetReturnType();
                            }
                            else
                                actionTaken = false;
                        }
                        else
                            actionTaken = false;
                        break;
                }

                if (usedEntry != null)
                    _host = usedEntry.Instance;
                        
                if (!actionTaken)
                    break;
                
                input = string.Join(".", parts.TakeSegment(i + 1));
            }
            
            return true;
        }

        protected override object GetInstance() => _host;
    }
}