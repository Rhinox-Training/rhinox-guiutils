using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Rhinox.GUIUtils.Editor
{
    public static class AttributeProcessorHelper
    {
        private static Dictionary<Type, IAttributeProcessor> _attributeProcessors;
        
        public static T FindAttributeInclusive<T>(Type type) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(type);
            return attrs.OfType<T>().FirstOrDefault();
        }

        public static T FindAttributeInclusive<T>(MemberInfo memberInfo, Type hostType = null) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(memberInfo, hostType);
            return attrs.OfType<T>().FirstOrDefault();
        }
        
        public static IEnumerable<T> FindAttributesInclusive<T>(Type type) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(type);
            return attrs.OfType<T>();
        }

        public static IEnumerable<T> FindAttributesInclusive<T>(MemberInfo memberInfo, Type hostType = null) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(memberInfo, hostType);
            return attrs.OfType<T>();
        }

        public static ICollection<Attribute> FindAllAttributesInclusive(Type type)
        {
            var attributes = type.GetCustomAttributes();
            var processor = FindProcessor(type);
            if (processor == null)
                return attributes;

            var totalAttrs = new List<Attribute>();
            processor.ProcessType(ref totalAttrs);
            
            totalAttrs.AddRange(attributes);

            
            ExpandIncludeMyAttributes(ref totalAttrs);
            
            return totalAttrs;
        }
        
        public static ICollection<Attribute> FindAllAttributesInclusive(MemberInfo info, Type hostType = null)
        {
            var attributes = info.GetCustomAttributes().ToArray();
            var processor = FindProcessor(hostType ?? info.ReflectedType);
            if (processor == null)
                return attributes;

            var totalAttrs = new List<Attribute>();
            processor.ProcessMember(info, ref totalAttrs);
            
            totalAttrs.AddRange(attributes);

            ExpandIncludeMyAttributes(ref totalAttrs);
            
            return totalAttrs;
        }

        private static void ExpandIncludeMyAttributes(ref List<Attribute> totalAttrs)
        {
            foreach (var attr in totalAttrs.ToArray())
            {
                var attrType = attr.GetType();
                if (attrType.GetCustomAttribute<IncludeMyAttributesAttribute>() == null)
                    continue;

                foreach (var includedAttr in attrType.GetCustomAttributes()
                             .Where(x => x.GetType() != typeof(IncludeMyAttributesAttribute)))
                    totalAttrs.Add(includedAttr);
            }
        }

        private static IAttributeProcessor FindProcessor(Type t)
        {
            if (_attributeProcessors == null)
            {
                _attributeProcessors = new Dictionary<Type, IAttributeProcessor>();
                
                var types = AppDomain.CurrentDomain.GetDefinedTypesOfType<IAttributeProcessor>();
                foreach (var type in types)
                {
                    var processor = Activator.CreateInstance(type) as IAttributeProcessor;
                    if (processor == null || processor.ManagedType == null)
                        continue;
                    
                    if (_attributeProcessors.ContainsKey(processor.ManagedType))
                    {
                        Debug.LogError($"ManagedType '{processor.ManagedType}' already has a processor");
                        continue;
                    }

                    _attributeProcessors.Add(processor.ManagedType, processor);
                }
            }

            if (_attributeProcessors.ContainsKey(t))
                return _attributeProcessors[t];
            return null;
        }
    }
}