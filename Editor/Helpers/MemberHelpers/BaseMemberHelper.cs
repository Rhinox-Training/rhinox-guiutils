using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseMemberHelper
    {
        protected string _errorMessage;
        protected Type _objectType;

        protected object _host;
        protected NewFrameHandler _newFrameHandler;
        
        /// <summary>Gets the type of the object.</summary>
        public Type ObjectType => this._objectType;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        protected virtual MemberTypes AllowedMembers => MemberTypes.Property | MemberTypes.Field | MemberTypes.Method;

        protected virtual bool TryParseInput(ref string input, out bool parameter)
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
                
                if (!TryParseParameter(input))
                    return false;
            }
            
            return true;
        }

        protected virtual bool TryParseParameter(string input)
        {
            return true;
        }

        protected virtual bool TryParseExpression(string input)
        {
            this._errorMessage = "Expressions are only supported with Odin Enabled";
            return false;
        }
        
        protected MemberInfo[] FindMembers(bool isStatic, MemberFilter filter, object filterCriteria = null)
        {
            var flags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = _objectType.FindMembers(
                AllowedMembers,
                flags,
                filter,
                filterCriteria
            );
            return members;
        }
    }
}