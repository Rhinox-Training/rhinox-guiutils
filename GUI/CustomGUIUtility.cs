using System.Reflection;
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public class CustomGUIUtility
    {
        public static int GetPermanentControlID()
        {
            var methodInfo = typeof(GUIUtility).GetMethod("GetPermanentControlID",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return (int) methodInfo.Invoke(null, null);
        }
    }
}