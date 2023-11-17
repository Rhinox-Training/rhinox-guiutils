using System.Reflection;
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public static class CustomGUIUtility
    {
        public const float Padding = 2.0f;
        public const float Indent = 15f;
        
        public static int GetPermanentControlID()
        {
            var methodInfo = typeof(GUIUtility).GetMethod("GetPermanentControlID",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (int) methodInfo.Invoke(null, null);
        }
        
        public static GUIContent CreateGUIContentForObject(object obj)
        {
            if (obj is Component component)
                return new GUIContent(component.gameObject.name);
            
            if (obj is UnityEngine.Object unityObj)
                return new GUIContent(unityObj.name);
            
            return new GUIContent(obj.ToString());
        }
    }
}