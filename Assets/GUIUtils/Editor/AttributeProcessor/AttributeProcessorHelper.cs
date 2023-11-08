using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

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

        public static T FindAttributeInclusive<T>(MemberInfo memberInfo) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(memberInfo);
            return attrs.OfType<T>().FirstOrDefault();
        }
        
        public static IEnumerable<T> FindAttributesInclusive<T>(Type type) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(type);
            return attrs.OfType<T>();
        }

        public static IEnumerable<T> FindAttributesInclusive<T>(MemberInfo memberInfo) where T : Attribute
        {
            var attrs = FindAllAttributesInclusive(memberInfo);
            return attrs.OfType<T>();
        }

        public static ICollection<Attribute> FindAllAttributesInclusive(Type type)
        {
            var attributes = type.GetCustomAttributes();
            var processor = FindProcessor(type);
            if (processor == null)
                return attributes;

            var totalAttrs = attributes.ToList();
            processor.ProcessType(ref totalAttrs);
            return totalAttrs;
        }
        
        public static ICollection<Attribute> FindAllAttributesInclusive(MemberInfo info)
        {
            var attributes = info.GetCustomAttributes().ToList();
            var processor = FindProcessor(info.DeclaringType);
            if (processor == null)
                return attributes;

            var totalAttrs = attributes;
            processor.ProcessMember(info, ref totalAttrs);
            return totalAttrs;
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