using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

[ResolverPriority(-100000.0)]
public class InheritClassAttributeAttributesAttributeProcessor : OdinAttributeProcessor
{
    public override bool CanProcessSelfAttributes(InspectorProperty property)
    {
        if (!base.CanProcessSelfAttributes(property))
            return false;
        
        return property.IsTreeRoot;
    }

    public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
    {
        base.ProcessSelfAttributes(property, attributes);
        
        for (int index = attributes.Count - 1; index >= 0; --index)
        {
            Type type = attributes[index].GetType();
            if (type.IsDefined(typeof (IncludeMyAttributesAttribute), false))
            {
                foreach (object customAttribute in type.GetCustomAttributes(false))
                {
                    if (customAttribute is AttributeUsageAttribute)
                        continue;
                    
                    if (customAttribute is IncludeMyAttributesAttribute)
                        continue;
                    
                    attributes.Add(customAttribute as Attribute);
                }

                attributes.RemoveAt(index);
            }
        }
    }
}
