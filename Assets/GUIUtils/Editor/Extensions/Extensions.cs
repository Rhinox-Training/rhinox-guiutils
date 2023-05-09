using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
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
        
        public static void AddItem(this GenericMenu menu, string path, Action<string> func, bool on = false)
        {
            menu.AddItem(new GUIContent(path), on, () => func(path));
        }
        
        public static void AddItem<T>(this GenericMenu menu, T item, Action<T> func, bool on = false)
        {
            menu.AddItem(new GUIContent(item.ToString()), on, () => func(item));
        }
        
        public static void AddItem<T>(this GenericMenu menu, T item, string name, Action<T> func, bool on = false)
        {
            menu.AddItem(new GUIContent(name), on, () => func(item));
        }


        public static GUILayoutOption[] Append(this GUILayoutOption[] options, params GUILayoutOption[] otherOptions)
        {
            var result = new GUILayoutOption[options.Length + otherOptions.Length];
            Array.Copy(options, result, options.Length);
            Array.Copy(otherOptions, 0, result, options.Length, otherOptions.Length);
            return result;
        }


        public class ExposedMenuItem
        {
            public GUIContent content;
            public bool separator;
            public bool on;

            public string Path => content.text;
        }
        
        private static MemberInfo _menuArrayListInfo;
        private static Type _innerMenuItemType;
        private static FieldInfo _contentFieldInfo;
        private static FieldInfo _onFieldInfo;
        private static FieldInfo _separatorFieldInfo;

        public static ExposedMenuItem[] GetItems(this GenericMenu menu)
        {
            if (_menuArrayListInfo == null)
                _menuArrayListInfo = typeof(GenericMenu)
                    .GetMember("menuItems", MemberTypes.Field | MemberTypes.Property, 
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault();
                

            return ((IEnumerable) _menuArrayListInfo?.GetValue(menu))?
                .OfType<object>()
                .Select(BuildExposedMenuItem)
                .ToArray();
        }

        private static ExposedMenuItem BuildExposedMenuItem(object internalUnityMenuItem)
        {
            if (_innerMenuItemType == null)
                _innerMenuItemType = typeof(GenericMenu).GetNestedType("MenuItem", BindingFlags.Public | BindingFlags.NonPublic);

            if (_contentFieldInfo == null)
                _contentFieldInfo = _innerMenuItemType.GetField("content", BindingFlags.Instance | BindingFlags.Public);
            if (_separatorFieldInfo == null)
                _separatorFieldInfo = _innerMenuItemType.GetField("separator", BindingFlags.Instance | BindingFlags.Public);
            if (_onFieldInfo == null)
                _onFieldInfo = _innerMenuItemType.GetField("on", BindingFlags.Instance | BindingFlags.Public);

            return new ExposedMenuItem()
            {
                content = (GUIContent) _contentFieldInfo.GetValue(internalUnityMenuItem),
                separator = (bool) _separatorFieldInfo.GetValue(internalUnityMenuItem),
                on = (bool) _onFieldInfo.GetValue(internalUnityMenuItem)
            };
        }

        public static Vector2 GetRectSize(this GenericMenu menu)
        {
            float width = 0.0f;
            float height = 0.0f;
            foreach (var item in menu.GetItems())
            {
                if (!string.IsNullOrEmpty(item.Path))
                {
                    var style = new GUIStyle("label")
                    {
                        fontSize = 11
                    };
                    var itemSize = style.CalcSize(item.content);
                    width = Mathf.Max(width, itemSize.x * 0.965f);
                    height += itemSize.y;
                }
                else
                    height += 1.0f;
            }

            const float horizontalPadding = 43.5f;
            const float verticalPadding = 2.0f;
            return new Vector2(width + 2.0f * horizontalPadding, height + 2.0f * verticalPadding);
        }

        public static void DropdownLeft(this GenericMenu menu, Rect rect)
        {
            menu.DropDown(rect.AlignRight(menu.GetRectSize().x));
        }
    }
}