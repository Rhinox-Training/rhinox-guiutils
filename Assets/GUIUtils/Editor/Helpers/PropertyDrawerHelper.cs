using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public static class PropertyDrawerHelper
    {
        private static Type _scriptAttributeUtilityType;
        public static Type ScriptAttributeUtilityType
            => _scriptAttributeUtilityType ?? (_scriptAttributeUtilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility"));
        
        private static MethodInfo _getDrawerTypeMethod;
        private static MethodInfo _getHandlerMethod;
        
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        public static Type GetDrawerTypeFor(Type type)
        {
            if (_getDrawerTypeMethod == null)
                _getDrawerTypeMethod = ScriptAttributeUtilityType.GetMethod("GetDrawerTypeForType", StaticFlags);
#if UNITY_2022_2_OR_NEWER
            return (Type) _getDrawerTypeMethod.Invoke(null, new object[] {type, false});
#else
            return (Type) _getDrawerTypeMethod.Invoke(null, new object[] {type});
#endif
        }

        public static object GetHandler(SerializedProperty property)
        {
            if (_getHandlerMethod == null)
                _getHandlerMethod = ScriptAttributeUtilityType.GetMethod("GetHandler", StaticFlags);
            // Returns internal class PropertyHandler
            return _getHandlerMethod.Invoke(null, new object[] { property });
        }
    }
}