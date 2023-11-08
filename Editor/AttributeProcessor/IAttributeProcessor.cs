using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public interface IAttributeProcessor
    {
        Type ManagedType { get; }
        void ProcessType(ref List<Attribute> attributes);
        void ProcessMember(MemberInfo memberInfo, ref List<Attribute> attributes);
    }
}