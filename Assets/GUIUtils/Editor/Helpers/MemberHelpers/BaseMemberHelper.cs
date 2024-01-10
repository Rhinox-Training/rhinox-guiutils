using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseMemberHelper
    {
        protected GenericHostInfo _hostInfo;
        protected string _errorMessage;
        protected Type _objectType;

        public bool HasError => !_errorMessage.IsNullOrEmpty();

        protected object _host;
        protected NewFrameHandler _newFrameHandler;
        
        /// <summary>Gets the type of the object.</summary>
        public Type ObjectType => this._objectType;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        protected virtual MemberTypes AllowedMembers => MemberTypes.Property | MemberTypes.Field | MemberTypes.Method;

        protected bool TryParseInput(ref string input, out bool parameter)
        {
            parameter = false;
            
            if (string.IsNullOrEmpty(input) || _objectType == null || input.Length <= 0)
                return false;
            
            if (input[0] == '@')
            {
                input = input.Substring(1);

                if (!TryParseExpression(input))
                    return false;
            }

            if (input[0] == '$')
            {
                input = input.Substring(1);
                parameter = true;
                
                if (!TryParseParameter(ref input))
                    return false;
            }
            
            return true;
        }

        protected virtual bool TryParseParameter(ref string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            
            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";
            const string PROPERTY_ID = "property";
            const string VALUE_ID = "value";
            
            GenericHostInfo relevantHostInfo = null;

            string[] parts = input.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; ++i)
            {
                bool actionTaken = true;

                switch (parts[i])
                {
                    case PROPERTY_ID:
                        if (_hostInfo != null)
                        {
                            _host = _hostInfo;
                            _objectType = _host?.GetType();
                        }
                        break;
                    case PARENT_ID:
                        if (relevantHostInfo != null)
                            relevantHostInfo = relevantHostInfo.Parent;
                        else if (_hostInfo != null)
                            relevantHostInfo = _hostInfo.Parent;
                        break;
                    case ROOT_ID:
                        if (relevantHostInfo != null)
                        {
                            relevantHostInfo = _hostInfo;

                            while (relevantHostInfo.Parent != null)
                                relevantHostInfo = relevantHostInfo.Parent;
                        }
                        break;
                    case VALUE_ID:
                        if (_hostInfo != null)
                        {
                            _host = _hostInfo.GetValue();
                            // _objectType = _host?.GetType();
                        }
                        break;
                    default:
                        relevantHostInfo = null;
                        if (i != parts.Length - 1) // if we're not at the last part
                        {
                            // try to resolve it
                            if (TryFindMember(parts[i], out MemberInfo info))
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

                if (relevantHostInfo != null)
                {
                    _host = relevantHostInfo.GetHost();
                    _objectType = relevantHostInfo.HostType;
                }
                        
                if (!actionTaken)
                    break;
                
                input = string.Join(".", parts.TakeSegment(i + 1));
            }
            
            return true;
        }

        protected virtual bool TryParseExpression(string input)
        {
            // TODO: We do not support Expressions; but we can pipe through to parameter parsing to partially support odin's syntax
            input = input.Substring(1);
            // parameter = true;
                
            if (TryParseParameter(ref input))
                return true;
            
            this._errorMessage = "Expressions are only supported with Odin Enabled";
            return false;
        }
        
        public bool DrawError()
        {
            if (_errorMessage.IsNullOrEmpty())
                return false;
                
            EditorGUILayout.HelpBox(_errorMessage, MessageType.Error, true);
            return true;
        }
        
        public bool DrawError(Rect rect)
        {
            if (_errorMessage.IsNullOrEmpty())
                return false;
                
            EditorGUI.HelpBox(rect, _errorMessage, MessageType.Error);
            return true;
        }
        
        protected bool TryFindMemberInHost<T>(string input, bool isStatic, out Func<T> staticGetter, out Func<object, T> instanceGetter)
        {
            staticGetter = null;
            instanceGetter = null;
            
            if (!TryFindMember(input, out MemberInfo info, isStatic, !isStatic))
                return false;

            if (!info.GetReturnType().InheritsFrom<T>())
                return false;

            if (isStatic)
                staticGetter = () => (T) info.GetValue(null);
            else
                instanceGetter = (i) => (T) info.GetValue(i);
            
            return true;
        }
        
        protected bool TryFindMember(string filter, out MemberInfo info, bool isStatic = false, bool includeExtensionMethods = false)
        {
            var flags = isStatic ? BindingFlags.Static : (BindingFlags.Static | BindingFlags.Instance);
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            return ReflectionUtility.TryGetMember(_objectType, AllowedMembers, filter, out info, flags, includeExtensionMethods);
        }
    }
}