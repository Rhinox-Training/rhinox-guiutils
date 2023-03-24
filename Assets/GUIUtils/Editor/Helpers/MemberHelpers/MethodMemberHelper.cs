using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public class MethodMemberHelper : BaseMemberHelper
    {
        protected Action _staticInvoker;
        protected Action<object> _instanceInvoker;
        
        protected override MemberTypes AllowedMembers => MemberTypes.Method;

        public MethodMemberHelper(object target, string input)
            : this(target == null, target?.GetType(), input)
        {
            _host = target;
        }
        
        public MethodMemberHelper(Type type, string input)
            : this(true, type, input)
        {
        }

        private MethodMemberHelper(bool isStatic, Type type, string input)
        {
            _objectType = type;

            if (!TryParseInput(ref input))
                return;

            var members = FindMembers(isStatic, (info, _) => info.Name == input);

            var mi = (MethodInfo) members.FirstOrDefault();
            
            if (mi == null)
                _errorMessage = $"Could not find method {input} on type {_objectType.Name}";
            else if (mi.IsStatic())
                _staticInvoker = () => mi.Invoke(null, new object[] {});
            else
                _instanceInvoker = (i) => mi.Invoke(i, new object[] {});
        }

        public void Invoke()
        {
            if (this._errorMessage != null)
                return;

            if (this._staticInvoker != null)
            {
                this._staticInvoker.Invoke();
                return;
            }

            if (_instanceInvoker != null)
            {
                _instanceInvoker.Invoke(_host);
                return;
            }
            
            return;
        }
    }
}