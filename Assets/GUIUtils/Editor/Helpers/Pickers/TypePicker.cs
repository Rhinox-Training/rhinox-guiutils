using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class TypePicker : SimplePicker<Type>
    {
        public TypePicker(Type baseType)
        {
            var options = TypeCache.GetTypesDerivedFrom(baseType);
            var filteredCollection = FilteredCollection.Create(FilterTypes(options), GetTypeName, GetTypeNamespace);
            InitData(filteredCollection);
        }
        
        public TypePicker(ICollection<Type> types)
        {
            var filteredCollection = FilteredCollection.Create(FilterTypes(types), GetTypeName, GetTypeNamespace);
            InitData(filteredCollection);
        }
        
        private static ICollection<Type> FilterTypes(ICollection<Type> types)
        {
            return types
                .Where(ValidateType)
                .ToArray();
        }

        private static bool ValidateType(Type t)
        {
            if (t.TryGetAttribute<CompilerGeneratedAttribute>(out _))
                return false;
            return true;
        }

        private static string GetTypeName(Type t)
        {
            return t.Name;
        }
        
        private static string GetTypeNamespace(Type t)
        {
            return t.Namespace;
        }
    }
}