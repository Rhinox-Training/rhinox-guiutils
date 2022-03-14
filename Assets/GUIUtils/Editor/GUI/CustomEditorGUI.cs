using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class CustomEditorGUI
    {
        private const int DEFAULT_LINE_WIDTH = 1; 
        
        public static void HorizontalLine(int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.HorizontalLine(CustomGUIStyles.BorderColor, lineWidth);

        public static void HorizontalLine(Color color, int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.DrawSolidRect(GUILayoutUtility.GetRect((float) lineWidth, (float) lineWidth, GUILayout.ExpandWidth(true)), color);

        public static void VerticalLine(int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.VerticalLine(CustomGUIStyles.BorderColor, lineWidth);

        public static void VerticalLine(Color color, int lineWidth = DEFAULT_LINE_WIDTH) => CustomEditorGUI.DrawSolidRect(GUILayoutUtility.GetRect((float) lineWidth, (float) lineWidth, GUILayout.ExpandHeight(true), GUILayout.Width((float)lineWidth)), color);

        public static void DrawSolidRect(Rect rect, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type != UnityEngine.EventType.Repaint)
                return;
            if (usePlaymodeTint)
            {
                EditorGUI.DrawRect(rect, color);
            }
            else
            {
                GUIContentHelper.PushColor(color);
                GUI.DrawTexture(rect, (Texture) EditorGUIUtility.whiteTexture);
                GUIContentHelper.PopColor();
            }
        }

        public static bool IconButton(Texture icon, int width = 18, int height = 18, string tooltip = "")
        {
            return CustomEditorGUI.IconButton(icon, (GUIStyle)null, width, height, tooltip);
        }

        public static bool IconButton(Texture icon, GUIStyle style, int width = 18, int height = 18, string tooltip = "")
        {
            style = style ?? CustomGUIStyles.IconButton;
            var tempContent = GUIContentHelper.TempContent(icon);
            return CustomEditorGUI.IconButton(
                GUILayoutUtility.GetRect(tempContent, style, GUILayout.ExpandWidth(false), GUILayout.Width((float) width), GUILayout.Height((float) height)), 
                icon, style, tooltip);
        }
        
        public static bool IconButton(Rect rect, Texture icon, GUIStyle style, string tooltip)
        {
#if ODIN_INSPECTOR
            return Sirenix.Utilities.Editor.SirenixEditorGUI.IconButton(rect, icon, style, tooltip);
#else
            GUIContent content = new GUIContent(icon, tooltip);
            style = style ?? CustomGUIStyles.IconButton;
            return GUI.Button(rect, content, style);
#endif
        }
        
    }
}