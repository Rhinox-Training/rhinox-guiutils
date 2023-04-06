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
        private static readonly GUIFrameAwareStack<Color> BackgroundColorStack = new GUIFrameAwareStack<Color>();
        private static readonly GUIFrameAwareStack<bool> GuiEnabled = new GUIFrameAwareStack<bool>();

#if UNITY_EDITOR
        private static readonly GUIFrameAwareStack<int> IndentLevelStack = new GUIFrameAwareStack<int>();
        private static readonly GUIFrameAwareStack<bool> HierarchyModeStack = new GUIFrameAwareStack<bool>();
        private static readonly GUIFrameAwareStack<float> LabelWidthStack = new GUIFrameAwareStack<float>();
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
        
        public static void PushBackgroundColor(Color color)
        {
            GUIContentHelper.BackgroundColorStack.Push(GUI.color);
            GUI.backgroundColor = color;
        }
        
        public static void PopBackgroundColor()
        {
            GUI.backgroundColor = GUIContentHelper.BackgroundColorStack.Pop();
        }
        
        public static void PushDisabled(bool disabled)
        {
            GUIContentHelper.GuiEnabled.Push(GUI.enabled);
            GUI.enabled = !disabled;
        }

        public static void PopDisabled()
        {
            GUI.enabled = GUIContentHelper.GuiEnabled.Pop();
        }
        
#if UNITY_EDITOR
        public static void PushIndentLevel(int indentLevel = -1)
        {
            GUIContentHelper.IndentLevelStack.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = indentLevel >= 0 ? indentLevel : EditorGUI.indentLevel + 1;
        }

        public static void PopIndentLevel()
        {
            EditorGUI.indentLevel = GUIContentHelper.IndentLevelStack.Pop();
        }
        
        public static void PushHierarchyMode(bool mode)
        {
            GUIContentHelper.HierarchyModeStack.Push(EditorGUIUtility.hierarchyMode);
            EditorGUIUtility.hierarchyMode = mode;
        }

        public static void PopHierarchyMode()
        {
            EditorGUIUtility.hierarchyMode = GUIContentHelper.HierarchyModeStack.Pop();
        }

        public static void PushLabelWidth(float width)
        {
            
            GUIContentHelper.LabelWidthStack.Push(EditorGUIUtility.labelWidth);
            EditorGUIUtility.labelWidth = width;
        }

        public static void PopLabelWidth()
        {
            EditorGUIUtility.labelWidth = GUIContentHelper.LabelWidthStack.Pop();
        }
#endif
    }
}