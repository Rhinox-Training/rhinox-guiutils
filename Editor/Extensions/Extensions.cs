using System;
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

        public static GUILayoutOption[] Append(this GUILayoutOption[] options, params GUILayoutOption[] otherOptions)
        {
            var result = new GUILayoutOption[options.Length + otherOptions.Length];
            Array.Copy(options, result, options.Length);
            Array.Copy(otherOptions, 0, result, options.Length, otherOptions.Length);
            return result;
        }
        
        public static Rect Expand(this Rect rect, float expand)
        {
            rect.x -= expand;
            rect.y -= expand;
            rect.height += expand * 2f;
            rect.width += expand * 2f;
            return rect;
        }
    }
}