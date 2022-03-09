using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class Extensions
    {
        public static void AddItem(this GenericMenu menu, string path, GenericMenu.MenuFunction func, bool on = false)
        {
            menu.AddItem(new GUIContent(path), on, func);
        }
    }
}