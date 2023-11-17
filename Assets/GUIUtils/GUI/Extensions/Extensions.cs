using UnityEngine;

namespace Rhinox.GUIUtils
{
    public static class Extensions
    {
        public static GUIStyle FindStyle(this GUISkin skin, string name, GUIStyle fallback)
        {
            return skin.FindStyle(name) ?? fallback;
        }

        public static GUIStyle FindStyle(this GUISkin skin, string name, string fallback)
            => FindStyle(skin, name, (GUIStyle) fallback);
    }
}