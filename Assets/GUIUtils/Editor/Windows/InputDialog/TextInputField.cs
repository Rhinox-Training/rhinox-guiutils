using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TextInputField : DialogInputField<string>
    {
        public TextInputField(string label, string tooltip = null, string initialValue = default(string))
            : base(label, tooltip, initialValue)
        {
        }

        protected override void DrawFieldValue(Rect rect)
        {
            SmartValue = EditorGUI.TextField(rect, SmartValue);
        }
    }
}