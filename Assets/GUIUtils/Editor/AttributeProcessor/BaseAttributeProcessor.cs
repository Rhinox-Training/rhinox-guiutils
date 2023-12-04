using System;
using System.Collections.Generic;
using System.Reflection;


namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseAttributeProcessor<T> :
#if ODIN_INSPECTOR
        Sirenix.OdinInspector.Editor.OdinAttributeProcessor<T>,
#endif
        IAttributeProcessor
    {
        public Type ManagedType => typeof(T);

#if ODIN_INSPECTOR
        public override void ProcessSelfAttributes(Sirenix.OdinInspector.Editor.InspectorProperty property,
            List<Attribute> attributes)
        {
            ProcessType(ref attributes);
        }
#endif

#if ODIN_INSPECTOR
        public override void ProcessChildMemberAttributes(Sirenix.OdinInspector.Editor.InspectorProperty parentProperty,
            MemberInfo member, List<Attribute> attributes)
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