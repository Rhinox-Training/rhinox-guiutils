using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public class MethodMemberHelper : BaseMemberHelper
    {
        private GenericHostInfo _hostInfo;
        
        protected MethodInfo _info;
        public Type MethodReturnType => _info != null ? _info.GetReturnType() : null;
        
        protected override MemberTypes AllowedMembers => MemberTypes.Method;

        public MethodMemberHelper(object host, string input)
            : this(host?.GetType(), input, host)
        {
        }
        
        public MethodMemberHelper(Type type, string input)
            : this(type, input, null)
        {
        }

        private MethodMemberHelper(Type type, string input, object host)
        {
            if (host is GenericHostInfo hostInfo)
            {
                _hostInfo = hostInfo;
                _host = _hostInfo.GetHost();
                _objectType = _host?.GetType();
            }
            else
            {
                _objectType = type;
                _host = host;
            }
            
            if (!TryParseInput(ref input, out _))
                return;

            var members = FindMembers(_host == null, (info, _) => info.Name == input);

            _info = (MethodInfo) members.FirstOrDefault();
            if (_info == null)
                _errorMessage = $"Could not find method {input} on type {_objectType.Name}";
        }

        public object Invoke(params object[] parameters)
        {
            if (this._errorMessage != null)
                return null;
            
            return this._info.Invoke(_host, parameters);
        }
        
#region EnforceSyntax
        public bool EnforceSyntax<T>() where T : Delegate
        {
            MethodInfo wantedMethod = typeof(T).GetMethod("Invoke");
            var parameters = wantedMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            return EnforceSyntax(wantedMethod.ReturnType, parameters);
        }

        public bool EnforceSyntax(int parameterIndex, Type parameterType)
        {
            if (_errorMessage != null)
                return false;
            var infos = _info.GetParameters();
            return EnforceSyntax(parameterIndex, parameterType, infos);
        }

        public bool EnforceSyntax(int numberOfParameters)
        {
            if (_errorMessage != null)
                return false;
            var infos = _info.GetParameters();
            return EnforceSyntax(numberOfParameters, infos);
        }

        public bool EnforceSyntax(bool hasReturnType)
        {
            bool isVoid = _info.ReturnType == typeof(void);
            if (hasReturnType == isVoid)
            {
                _errorMessage = $"Expected {(hasReturnType ? "a" : "no")} return value";
                return false;
            }
            
            return true;
        }
        
        public bool EnforceSyntax(Type returnType, params Type[] wantedParameters)
        {
            if (_errorMessage != null)
                return false;
            
            if (_info.ReturnType != returnType)
            {
                _errorMessage = $"Expected return type {returnType}; Received {_info.ReturnType}";
                return false;
            }

            var parameters = _info.GetParameters();

            if (wantedParameters.Length != parameters.Length)
            {
                _errorMessage = $"Expected the following parameters: " +
                                $"{string.Join(", ", wantedParameters.Select(x => x.Name))};" +
                                $"Received {string.Join(", ", parameters.Select(x => x.ParameterType.Name))}";
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var wantedType = wantedParameters[i];
                if (!EnforceSyntax(i, wantedType, parameters))
                    return false;
            }

            return true;
        }

        private bool EnforceSyntax(int numberOfParameters, ParameterInfo[] infos)
        {
            if (numberOfParameters == infos.Length)
                return true;
            
            _errorMessage = $"Expected {numberOfParameters} parameters; Received {infos.Length}";
            return false;
        }
        
        private bool EnforceSyntax(int parameterIndex, Type parameterType, ParameterInfo[] infos)
        {
            var info = infos.GetOrDefault(parameterIndex, null);
            if (info != null && info.ParameterType == parameterType)
                return true;
            
            _errorMessage = $"Expected a(n) {parameterType} for parameter nr. {parameterIndex}; Received {info?.ParameterType.Name ?? "None"}";
            return false;
        }
#endregion
    }
}