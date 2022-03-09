using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Odin.Editor
{
    internal static class TooltipClassCache
    {
        class TooltipClassInfo : Dictionary<string, string>
        {
            public static TooltipClassInfo CreateFrom(Type t)
            {
                var fields = t.GetFields(Flags.StaticAnyVisibility)
                    .Where(IsValidField);
                
                var info = new TooltipClassInfo();
                foreach (var field in fields)
                    info[field.Name] = field.GetValue(null).ToString();
                return info;
            }

            private static bool IsValidField(FieldInfo arg)
            {
                return arg.FieldType == typeof(string);
            }
        }

        static Dictionary<Type, TooltipClassInfo> _typeCache;

        public static string GetTooltip(Type type, string property)
        {
            if (_typeCache == null) BuildCache();
            
            if (!_typeCache.ContainsKey(type)) return null;
            
            var info = _typeCache[type];

            if (!info.ContainsKey(property))
                return null;
            return info[property];
        }
        
        static List<Type> GetFilteredType()
        {
            var types = new List<Type>();

            var scripts = MonoImporter.GetAllRuntimeMonoScripts();
            foreach (var script in scripts)
            {
                var type = script.GetClass();
                if (!IsTypeValid(type)) continue;
                
                if (!types.Contains(type))
                    types.Add(type);
            }

            return types;
        }

        static bool IsTypeValid(Type type)
        {
            if (type != null)
                return type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject));
            
            return false;
        }
        
        static void RegisterTooltipDefinitions(List<Type> types)
        {
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                var tooltipType = GetTooltipType(type);
                if (tooltipType == null) continue;

                _typeCache[type] = TooltipClassInfo.CreateFrom(tooltipType);
            }
        }

        private static Type GetTooltipType(Type type)
        {
            foreach (var t in type.GetNestedTypes(Flags.StaticAnyVisibility))
            {
                if (t.Name == "Tooltips" || t.Name == "ToolTips")
                    return t;
            }

            return null;
        }

        private static void BuildCache()
        {
            var types = GetFilteredType();

            _typeCache = new Dictionary<Type, TooltipClassInfo>();
            RegisterTooltipDefinitions(types);
        }
    }
    
    internal class TooltipsProcessor<T> : OdinAttributeProcessor<T> where T : Object
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
            
            var tooltip = TooltipClassCache.GetTooltip(parentProperty.ParentType, member.Name);

            if (!string.IsNullOrWhiteSpace(tooltip) && attributes.All(IsNotTooltipAttribute))
                attributes.Add(new TooltipAttribute(tooltip));
        }

        private bool IsNotTooltipAttribute(Attribute arg)
        {
            return !(arg is TooltipAttribute);
        }
    }
}