using System;
using System.Reflection;
using UnityEditor.Experimental;

namespace Rhinox.GUIUtils.Editor
{
    public static class ExposedEditorResources
    {
        private static MethodInfo _getStyleMethod;
        private static Type _styleBlockType;
        private static MethodInfo _getFloatMethod;
        private static object _defaultStateArray;

        public static float GetFloat(string styleSheetName, int fieldId)
        {
            if (_getStyleMethod == null)
            {
                var styleStateType = typeof(EditorResources).Assembly.GetType("UnityEditor.StyleSheets.StyleState");
                var styleStateArrayType = styleStateType.MakeArrayType();
                _defaultStateArray = Array.CreateInstance(styleStateType, 0);
                _getStyleMethod =
                    typeof(EditorResources).GetMethod("GetStyle", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        Type.DefaultBinder, new Type[] {typeof(string), styleStateArrayType}, null);

                _styleBlockType = typeof(EditorResources).Assembly.GetType("UnityEditor.StyleSheets.StyleBlock");
                _getFloatMethod = _styleBlockType.GetMethod("GetFloat", new Type[] {typeof(int), typeof(float)});
            }

            object styleBlock = _getStyleMethod.Invoke(null, new object[] {styleSheetName, _defaultStateArray});
            object defaultVal =_getFloatMethod.GetParameters()[1].DefaultValue;
            return (float)_getFloatMethod.Invoke(styleBlock, new object[] {fieldId, defaultVal});
        }
    }
}