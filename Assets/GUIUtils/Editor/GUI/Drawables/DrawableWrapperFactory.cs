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
        public delegate WrapperDrawable WrapperCreator(Attribute attribute, IOrderedDrawable drawable);

        private static readonly Dictionary<Type, WrapperCreator> _builderByAttribute = new Dictionary<Type, WrapperCreator>();
        private static readonly Dictionary<Type, int> _priorityByAttributeType = new Dictionary<Type, int>();

        private static bool _initialized;

        private static void Initialize()
        {
            var targets = TypeCache.GetMethodsWithAttribute<WrapDrawerAttribute>();

            foreach (var target in targets)
            {
                var attr = target.GetCustomAttribute<WrapDrawerAttribute>();
                Register(attr.AttributeType, target, attr.Priority);
            }
            _initialized = true;
        }

        public static void Register(Type attributeType, WrapperCreator creator, int priority = 0)
        {
            _builderByAttribute.Add(attributeType, creator);
            _priorityByAttributeType.Add(attributeType, priority);
        }
        
        private static void Register(Type attributeType, MethodInfo info, int priority = 0)
        {
            WrapperDrawable Creator(Attribute targetAttr, IOrderedDrawable drawable)
            {
                // TODO, idk... better
                return (WrapperDrawable)info.Invoke(null, new object[] { targetAttr, drawable });
            }

            Register(attributeType, Creator, priority);
        }

        public static IOrderedDrawable TryWrapDrawable(IOrderedDrawable drawable, IEnumerable<Attribute> attributes)
        {
            var attrs = attributes.ToList();
            attrs.SortByDescending(x => _priorityByAttributeType.GetOrDefault(x.GetType()));
            
            foreach (var attr in attrs)
            {
                if (TryCreateWrapper(attr, drawable, out WrapperDrawable wrappedDrawable))
                    drawable = wrappedDrawable;
            }
            return drawable;
        }

        private static bool TryCreateWrapper(Attribute attribute, IOrderedDrawable drawable, out WrapperDrawable wrapperDrawable)
        {
            if (!_initialized)
                Initialize();

            var attrType = attribute.GetType();
            if (!_builderByAttribute.ContainsKey(attrType))
            {
                wrapperDrawable = null;
                return false;
            }

            var creator = _builderByAttribute[attrType];
            wrapperDrawable = creator?.Invoke(attribute, drawable);
            return true;
        }
    }
}