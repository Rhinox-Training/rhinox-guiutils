using System;
using System.Collections.Generic;
using System.Reflection;
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
        
        public WrapDrawerAttribute(Type attributeType)
        {
            AttributeType = attributeType;
        }
    }

    public static class DrawableWrapperFactory
    {
        public delegate WrapperDrawable WrapperCreator(Attribute attribute, IOrderedDrawable drawable);

        private static readonly Dictionary<Type, WrapperCreator> _builderByAttribute = new Dictionary<Type, WrapperCreator>();

        private static bool _initialized;

        private static void Initialize()
        {
            var targets = TypeCache.GetMethodsWithAttribute<WrapDrawerAttribute>();

            foreach (var target in targets)
            {
                var attr = target.GetCustomAttribute<WrapDrawerAttribute>();
                Register(attr.AttributeType, target);
            }
            _initialized = true;
        }

        public static void Register(Type attributeType, WrapperCreator creator)
        {
            _builderByAttribute.Add(attributeType, creator);
        }
        
        private static void Register(Type attributeType, MethodInfo info)
        {
            _builderByAttribute.Add(attributeType, (targetAttr, drawable) =>
            {
                // TODO, idk better
                return (WrapperDrawable) info.Invoke(null, new object[] { targetAttr, drawable });
            });
        }

        public static bool TryCreateWrapper(Attribute attribute, IOrderedDrawable drawable, out WrapperDrawable wrapperDrawable)
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