#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public static class GUIContentHelper
    {
        private static readonly GUIContent _tempContent = new GUIContent("");
        
        private static readonly GUIFrameAwareStack<Color> ColorStack = new GUIFrameAwareStack<Color>();
#if UNITY_EDITOR
        private static readonly GUIFrameAwareStack<int> IndentLevelStack = new GUIFrameAwareStack<int>();
#endif

        public static GUIContent TempContent(string label, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;
            return _tempContent;
        }
        
        public static GUIContent TempContent(Texture image, string tooltip = null)
        {
            _tempContent.image = image;
            _tempContent.text = null;
            _tempContent.tooltip = tooltip;
            return _tempContent;
        }
        
        public static GUIContent TempContent(string label, Texture image, string tooltip = null)
        {
            _tempContent.image = image;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;
            return _tempContent;
        }
        
        // =============================================================================================================
        // GUIStyle extensions
        
        public static float CalcMinWidth(this GUIStyle style, string label, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            style.CalcMinMaxWidth(_tempContent, out float min, out float max);
            return min;
        }
        
        public static float CalcMaxWidth(this GUIStyle style, string label, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            style.CalcMinMaxWidth(_tempContent, out float min, out float max);
            return max;
        }
        
        
        public static void CalcMinMaxWidth(this GUIStyle style, string label, out float min, out float max, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            style.CalcMinMaxWidth(_tempContent, out min, out max);
        }

        public static Vector2 CalcSize(this GUIStyle style, string label, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            return style.CalcSize(_tempContent);
        }

        // =============================================================================================================
        // Label helpers
        
        public static float CalcMinLabelWidth(string label, string tooltip = null)
        {
            return GUI.skin.label.CalcMinWidth(label, tooltip);
        }
        
        public static float CalcMaxLabelWidth(string label, string tooltip = null)
        {
            return GUI.skin.label.CalcMaxWidth(label, tooltip);
        }
        
        public static void CalcMinMaxLabelWidth(string label, out float min, out float max, string tooltip = null)
        {
            GUI.skin.label.CalcMinMaxWidth(label, out min, out max, tooltip);
        }
        
        // =============================================================================================================
        // General helpers
        
        public static void PushColor(Color color)
        {
            GUIContentHelper.ColorStack.Push(GUI.color);
            GUI.color = color;
        }

        public static void PopColor()
        {
            GUI.color = GUIContentHelper.ColorStack.Pop();
        }
        
#if UNITY_EDITOR
        public static void PushIndentLevel(int indentLevel)
        {
            GUIContentHelper.IndentLevelStack.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = indentLevel;
        }

        public static void PopIndentLevel()
        {
            EditorGUI.indentLevel = GUIContentHelper.IndentLevelStack.Pop();
        }
#endif
    }
}