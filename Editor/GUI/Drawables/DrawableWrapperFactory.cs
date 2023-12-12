using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Only apply to method with the following signature:
    /// WrapperDrawable Method(Attribute attribute, IOrderedDrawable drawable)
    /// </summary>
    public class WrapDrawerAttribute : Attribute
    {
        public Type AttributeType;
        public int Priority;
        
        public WrapDrawerAttribute(Type attributeType, int priority = 0)
        {
            AttributeType = attributeType;
            Priority = priority;
        }
    }

    public static class DrawableWrapperFactory
    {
        public struct AttributeBuilder
        {
            public int Priority;
            public WrapperCreator Creator;
        }
        
        public delegate BaseWrapperDrawable WrapperCreator(Attribute attribute, IOrderedDrawable drawable);

        private static readonly Dictionary<Type, ICollection<AttributeBuilder>> _buildersByAttribute = new Dictionary<Type, ICollection<AttributeBuilder>>();

        private static bool _initialized;

        private static void Initialize()
        {
            var targets = TypeCache.GetMethodsWithAttribute<WrapDrawerAttribute>();

            foreach (var target in targets)
            {
                var attr = AttributeProcessorHelper.FindAttributeInclusive<WrapDrawerAttribute>(target);
                Register(attr.AttributeType, target, attr.Priority);
            }
            _initialized = true;
        }

        public static void Register(Type attributeType, WrapperCreator creator, int priority = 0)
        {
            if (!_buildersByAttribute.ContainsKey(attributeType))
                _buildersByAttribute.Add(attributeType, new List<AttributeBuilder>());
            
            _buildersByAttribute[attributeType].Add(new AttributeBuilder
            {
                Creator = creator,
                Priority = priority
            });
        }
        
        private static void Register(Type attributeType, MethodInfo info, int priority = 0)
        {
            BaseWrapperDrawable Creator(Attribute targetAttr, IOrderedDrawable drawable)
            {
                // TODO, idk... better
                return (BaseWrapperDrawable)info.Invoke(null, new object[] { targetAttr, drawable });
            }

            Register(attributeType, Creator, priority);
        }

        public static IOrderedDrawable TryWrapDrawable(IOrderedDrawable drawable, IEnumerable<Attribute> attributes)
        {
            if (drawable == null)
                return null;
            
            if (!_initialized)
                Initialize();


            if (!drawable.HostInfo.CanSetValue() && drawable.HostInfo.GetReturnType().IsValueType)
                attributes = attributes.Append(new Sirenix.OdinInspector.ReadOnlyAttribute());
            
            var builderPairs = attributes
                .Distinct() // IDK why but it's needed? are Attribute's Equal overridden?
                .ToDictionary(
                    x => x,
                    x => _buildersByAttribute.GetOrDefault(x.GetType(), Array.Empty<AttributeBuilder>())
                )
                .Flatten(x => x.Value.Select(y => (Attribute : x.Key, Builder: y)))
                .ToList();
            
            var orderedBuilderPairs = builderPairs.OrderByDescending(x => x.Builder.Priority);

            foreach (var pair in orderedBuilderPairs)
            {
                var creator = pair.Builder.Creator;
                var targetDrawable = creator.Invoke(pair.Attribute, drawable);
                if (targetDrawable != null)
                    drawable = targetDrawable;
            }

            return drawable;
        }
    }
}