using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class CustomEditorGUI
    {
        private const int DEFAULT_LINE_WIDTH = 1;
        private const int DEFAULT_ICON_WIDTH = 22;
        private const int DEFAULT_ICON_HEIGHT = 18;

        private static object _parentView = null;
        private static PropertyInfo _screenPosProp;
        private static MethodInfo _toolbarSearchFieldMethodInfo;

        public static Rect GetEditorWindowRect()
        {
            if (_parentView == null || _screenPosProp == null)
            {
                var editorAssy = typeof(UnityEditor.Editor).Assembly;
                var toolbarType = editorAssy.GetType("UnityEditor.Toolbar");
                var singletonToolbar = toolbarType.GetField("get",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var toolbar = singletonToolbar.GetValue(null);

                var viewType = editorAssy.GetType("UnityEditor.View");
                var parentProp = viewType.GetProperty("parent",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _screenPosProp = viewType.GetProperty("screenPosition", BindingFlags.Instance | BindingFlags.Public);
                _parentView = parentProp.GetValue(toolbar);

                if (_screenPosProp == null)
                    return default(Rect);
            }

            Rect editorWindowRect = _screenPosProp.GetValue(_parentView) is Rect
                ? (Rect) _screenPosProp.GetValue(_parentView)
                : default(Rect);

            return editorWindowRect;
        }

        public static EditorWindow CurrentWindow()
        {
            var guiViewType = typeof (UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView");
            var getProp = guiViewType.GetProperty("get", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (getProp != null)
                return getProp.GetValue(null) as EditorWindow;
            return null;
        }

        public static Rect GetTopLevelLayoutRect()
        {
            var layoutUtilType = typeof(UnityEngine.GUILayoutUtility);
            var currentField = layoutUtilType.GetField("current", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var layoutData = currentField.GetValue(null);

            if (layoutData == null)
                return default(Rect);

            var layoutType =
                typeof(UnityEngine.GUILayoutUtility).GetNestedType("LayoutCache",
                    BindingFlags.Public | BindingFlags.NonPublic);
            var topLevelField = layoutType.GetField("topLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var topLevelInstance = topLevelField.GetValue(layoutData);

            if (topLevelInstance == null)
                return default(Rect);
                
            var groupType = layoutUtilType.Assembly.GetType("UnityEngine.GUILayoutEntry");
            var rectField = groupType.GetField("rect", BindingFlags.Instance | BindingFlags.Public);
            return rectField.GetValue(topLevelInstance) is Rect ? (Rect) rectField.GetValue(topLevelInstance) : default;
        }
        
        public static Rect GetVisibleRect()
        {
            string fullTypeName = "UnityEngine.GUIClip";
            Type t = typeof(GUILayout).Assembly.GetType(fullTypeName);
            var propInfo = t.GetProperty("visibleRect", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (propInfo == null)
                return default(Rect);
            return (Rect)propInfo.GetValue(null);
        }

        public static void EndEditingActiveTextField()
        {
            var methodInfo = typeof(EditorGUI).GetMethod("EndEditingActiveTextField", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            methodInfo.Invoke(null, null);
        }

        public static bool HasCurrentWindowKeyFocus()
        {
            var methodInfo = typeof(EditorGUIUtility).GetMethod("HasCurrentWindowKeyFocus", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return (bool)methodInfo.Invoke(null, null);
        }
        
        public static void RemoveFocusControl()
        {
            GUIUtility.hotControl = 0;
            DragAndDrop.activeControlID = 0;
            GUIUtility.keyboardControl = 0;
        }

        public static void HorizontalLine(int thickness = DEFAULT_LINE_WIDTH, float? width = null) =>
            CustomEditorGUI.HorizontalLine(CustomGUIStyles.BorderColor, thickness, width);

        public static void HorizontalLine(Color color, int thickness = DEFAULT_LINE_WIDTH, float? width = null)
        {
            if (width.HasValue)
                CustomEditorGUI.DrawSolidRect(GUILayoutUtility.GetRect(width.Value, thickness, GUILayout.Width(width.Value)), color);
            else
                CustomEditorGUI.DrawSolidRect(GUILayoutUtility.GetRect(thickness, thickness, GUILayout.ExpandWidth(true)), color);
        }


        public static void HorizontalLine(Rect r, Color color, int lineWidth = DEFAULT_LINE_WIDTH)
        {
            r.height = lineWidth;
            CustomEditorGUI.DrawSolidRect(r, color);
        }

        public static void VerticalLine(int thickness = DEFAULT_LINE_WIDTH) =>
            CustomEditorGUI.VerticalLine(CustomGUIStyles.BorderColor, thickness);

        public static void VerticalLine(Color color, int thickness = DEFAULT_LINE_WIDTH) =>
            CustomEditorGUI.DrawSolidRect(
                GUILayoutUtility.GetRect(thickness, thickness, GUILayout.ExpandHeight(true),
                    GUILayout.Width(thickness)), color);

        public static void DrawSolidRect(Rect rect, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            if (usePlaymodeTint)
            {
                EditorGUI.DrawRect(rect, color);
            }
            else
            {
                GUIContentHelper.PushColor(color);
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                GUIContentHelper.PopColor();
            }
        }

        /// <summary>Draws borders around a rect.</summary>
        /// <param name="rect">The rect.</param>
        /// <param name="borderWidth">The width of the border on all sides.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int borderWidth, bool usePlaymodeTint = true) => DrawBorders(rect,
            borderWidth, borderWidth, borderWidth, borderWidth, CustomGUIStyles.BorderColor, usePlaymodeTint);

        /// <summary>Draws borders around a rect.</summary>
        /// <param name="rect">The rect.</param>
        /// <param name="borderWidth">The width of the border on all sides.</param>
        /// <param name="color">The color of the border.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int borderWidth, Color color, bool usePlaymodeTint = true) =>
            DrawBorders(rect, borderWidth, borderWidth, borderWidth, borderWidth, color, usePlaymodeTint);

        /// <summary>Draws borders around a rect.</summary>
        /// <param name="rect">The rect.</param>
        /// <param name="left">The left size.</param>
        /// <param name="right">The right size.</param>
        /// <param name="top">The top size.</param>
        /// <param name="bottom">The bottom size.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(
            Rect rect,
            int left,
            int right,
            int top,
            int bottom,
            bool usePlaymodeTint = true)
        {
            DrawBorders(rect, left, right, top, bottom, CustomGUIStyles.BorderColor, usePlaymodeTint);
        }

        /// <summary>Draws borders around a rect.</summary>
        /// <param name="rect">The rect.</param>
        /// <param name="left">The left size.</param>
        /// <param name="right">The right size.</param>
        /// <param name="top">The top size.</param>
        /// <param name="bottom">The bottom size.</param>
        /// <param name="color">The color of the borders.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(
            Rect rect,
            int left,
            int right,
            int top,
            int bottom,
            Color color,
            bool usePlaymodeTint = true)
        {
            if (Event.current.type != UnityEngine.EventType.Repaint)
                return;
            if (left > 0)
                DrawSolidRect(rect.SetWidth(left), color, usePlaymodeTint);
            if (top > 0)
                DrawSolidRect(rect.SetHeight(top), color, usePlaymodeTint);
            if (right > 0)
            {
                Rect rect1 = rect;
                rect1.x += rect.width - right;
                rect1.width = right;
                DrawSolidRect(rect1, color, usePlaymodeTint);
            }

            if (bottom <= 0)
                return;
            Rect rect2 = rect;
            rect2.y += rect.height - bottom;
            rect2.height = bottom;
            DrawSolidRect(rect2, color, usePlaymodeTint);
        }

        public static bool IconButton(Texture icon, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT, string tooltip = "")
        {
            return IconButton(icon, null, width, height, tooltip);
        }

        public static bool IconButton(Texture icon, GUIStyle style = null, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT, string tooltip = "")
        {
            return IconButton(GUIContentHelper.TempContent(icon, tooltip), style, width, height);
        }
        
        public static bool IconButton(HoverTexture icon, GUIStyle style = null, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT, string tooltip = "")
        {
            style = style ?? CustomGUIStyles.IconButton;
            var rect = GUILayoutUtility.GetRect(GUIContentHelper.TempContent(icon.Normal, tooltip), style, GUILayout.ExpandWidth(false), GUILayout.Width(width), GUILayout.Height(height));
            return IconButton(rect, icon, tooltip, style);
        }

        public static bool IconButton(GUIContent content, GUIStyle style = null, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT)
        {
            style = style ?? CustomGUIStyles.IconButton;
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(false), GUILayout.Width(width), GUILayout.Height(height));
            return IconButton(rect, content, style);
        }
        
        public static bool IconButton(Rect rect, HoverTexture icon, string tooltip = "", GUIStyle style = null)
        {
            var tex = GUI.enabled ? icon.GetNeededTexture(rect, out _) : icon.Normal;
            style = style ?? CustomGUIStyles.IconButton;
            return IconButton(rect, GUIContentHelper.TempContent(tex, tooltip), style);
        }
        
        public static bool IconButton(Rect rect, Texture icon, string tooltip = "", GUIStyle style = null)
        {
            style = style ?? CustomGUIStyles.IconButton;
            return IconButton(rect, GUIContentHelper.TempContent(icon, tooltip), style);
        }

        public static bool IconButton(Rect rect, GUIContent content, GUIStyle style = null)
        {
            style = style ?? CustomGUIStyles.IconButton;
            return GUI.Button(rect, content, style);
        }

        public static bool ToolbarButton(string text, string tooltip = "")
        {
            return ToolbarButton(GUIContentHelper.TempContent(text, tooltip));
        }

        public static bool ToolbarButton(GUIContent content)
        {
            return GUILayout.Button(content, 
                CustomGUIStyles.ToolbarButtonCentered, 
                GUILayout.Height(22), GUILayout.ExpandWidth(false));
        }
        
            /// <summary>
        /// Begins a horizontal toolbar. Remember to end with <see cref="M:Sirenix.Utilities.Editor.SirenixEditorGUI.EndHorizontalToolbar" />.
        /// </summary>
        /// <param name="height">The height of the toolbar.</param>
        /// <param name="paddingTop">Padding for the top of the toolbar.</param>
        /// <returns>The rect of the horizontal toolbar.</returns>
        public static Rect BeginHorizontalToolbar(float height = 22f, int paddingTop = 4) => BeginHorizontalToolbar(CustomGUIStyles.ToolbarBackground, height, paddingTop);

        /// <summary>
        /// Begins a horizontal toolbar. Remember to end with <see cref="M:Sirenix.Utilities.Editor.SirenixEditorGUI.EndHorizontalToolbar" />.
        /// </summary>
        /// <param name="style">The style for the toolbar.</param>
        /// <param name="height">The height of the toolbar.</param>
        /// <param name="topPadding">The top padding.</param>
        /// <returns>The rect of the horizontal toolbar.</returns>
        public static Rect BeginHorizontalToolbar(GUIStyle style, float height = 22f, int topPadding = 4)
        {
            //SirenixEditorGUI.currentDrawingToolbarHeight = height;
            Rect rect = EditorGUILayout.BeginHorizontal(style, GUILayout.Height(height), GUILayout.ExpandWidth(false));
            GUIContentHelper.PushHierarchyMode(false);
            GUIContentHelper.PushIndentLevel(0);
            return rect;
        }

        /// <summary>
        /// Ends a horizontal toolbar started by <see cref="!:BeginHorizontalToolbar(int, int)" />.
        /// </summary>
        public static void EndHorizontalToolbar()
        {
            if (Event.current.type == EventType.Repaint)
            {
                Rect currentLayoutRect = GetTopLevelLayoutRect();
                --currentLayoutRect.yMin;
                DrawBorders(currentLayoutRect, 1);
            }
            GUIContentHelper.PopIndentLevel();
            GUIContentHelper.PopHierarchyMode();
            EditorGUILayout.EndHorizontal();
        }

        public static string ToolbarSearchField(string searchText, params GUILayoutOption[] layoutOptions)
        {
            if (_toolbarSearchFieldMethodInfo == null)
            {
                var methods = typeof(EditorGUILayout).GetMethods( BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                _toolbarSearchFieldMethodInfo = methods.FirstOrDefault(x => x.Name.Equals(nameof(ToolbarSearchField)) && x.GetParameters().Length == 2);
            }

            string result = _toolbarSearchFieldMethodInfo.Invoke(null, new object[] {searchText, layoutOptions}) as string;
            return result ?? string.Empty;
        }
        
        public static void SelectObject(UnityEngine.Object obj)
        {
            if (obj == null)
                return;
            
            if (AssetDatabase.Contains(obj) && !AssetDatabase.IsMainAsset(obj))
            {
                UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(obj));
                if (o is Component)
                    o = (o as Component).gameObject;
                Selection.activeObject = o;
            }
            else
            {
                if (obj is Component)
                    obj = (obj as Component).gameObject;
                Selection.activeObject = obj;
            }
        }
    }
}