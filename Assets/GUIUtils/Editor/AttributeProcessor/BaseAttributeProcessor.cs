using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseAttributeProcessor<T> : IAttributeProcessor
#if ODIN_INSPECTOR
        , OdinAttributeProcessor<T>
#endif
    {
        public Type ManagedType => typeof(T);
        
#if ODIN_INSPECTOR
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            ProcessType(ref attributes);
        }
#endif

#if ODIN_INSPECTOR
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            ProcessMember(member, ref attributes);
        }
#endif

        public virtual void ProcessType(ref List<Attribute> attributes)
        {
            
        }

        public virtual void ProcessMember(MemberInfo memberInfo, ref List<Attribute> attributes)
        {
            
        }
    }
}