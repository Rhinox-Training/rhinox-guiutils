using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class CustomEditorGUI
    {
        private const int DEFAULT_LINE_WIDTH = 1;
        private const int DEFAULT_ICON_WIDTH = 22;
        private const int DEFAULT_ICON_HEIGHT = 18;

        public static void HorizontalLine(int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.HorizontalLine(CustomGUIStyles.BorderColor, lineWidth);

        public static void HorizontalLine(Color color, int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.DrawSolidRect(GUILayoutUtility.GetRect((float) lineWidth, (float) lineWidth, GUILayout.ExpandWidth(true)), color);

        public static void VerticalLine(int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.VerticalLine(CustomGUIStyles.BorderColor, lineWidth);

        public static void VerticalLine(Color color, int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.DrawSolidRect(GUILayoutUtility.GetRect((float) lineWidth, (float) lineWidth, GUILayout.ExpandHeight(true), GUILayout.Width((float)lineWidth)), color);

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

        public static bool IconButton(Texture icon, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT, string tooltip = "")
        {
            return IconButton(icon, null, width, height, tooltip);
        }

        public static bool IconButton(Texture icon, GUIStyle style = null, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT, string tooltip = "")
        {
            return IconButton(GUIContentHelper.TempContent(icon, tooltip), style, width, height);
        }
        
        public static bool IconButton(GUIContent content, GUIStyle style = null, int width = DEFAULT_ICON_WIDTH, int height = DEFAULT_ICON_HEIGHT)
        {
            style = style ?? CustomGUIStyles.IconButton;
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(false), GUILayout.Width(width), GUILayout.Height(height));
            return IconButton(rect, content, style);
        }
        
        private static bool IconButton(Rect rect, GUIContent content, GUIStyle style = null)
        {
            style = style ?? CustomGUIStyles.IconButton;
            return GUI.Button(rect, content, style);
        }
    }
}