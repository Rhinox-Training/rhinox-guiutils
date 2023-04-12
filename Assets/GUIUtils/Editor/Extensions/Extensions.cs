using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
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

        
        private static MemberInfo _menuArrayListInfo;
        public static MenuItem[] GetItems(this GenericMenu menu)
        {
            if (_menuArrayListInfo == null)
                _menuArrayListInfo = typeof(GenericMenu)
                    .GetMember("menuItems", MemberTypes.Field | MemberTypes.Property, 
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault();
                

            return ((IEnumerable) _menuArrayListInfo?.GetValue(menu))?.OfType<MenuItem>().ToArray();
        }
    }
}