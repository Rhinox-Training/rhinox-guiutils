using UnityEngine;

namespace Rhinox.GUIUtils
{
    public static class GUIContentHelper
    {
        private static readonly GUIContent _tempContent = new GUIContent("");

        public static float CalcMinLabelWidth(string label, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            GUI.skin.label.CalcMinMaxWidth(_tempContent, out float min, out float max);
            return min;
        }
        
        public static float CalcMaxLabelWidth(string label, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            GUI.skin.label.CalcMinMaxWidth(_tempContent, out float min, out float max);
            return max;
        }
        
        public static void CalcMinMaxLabelWidth(string label, out float min, out float max, string tooltip = null)
        {
            _tempContent.image = null;
            _tempContent.text = label;
            _tempContent.tooltip = tooltip;

            GUI.skin.label.CalcMinMaxWidth(_tempContent, out min, out max);
        }
    }
}