using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class Int32InputField : DialogInputField<int>
    {
        public Int32InputField(string label, string tooltip = null, int initialValue = default(int))
            : base(label, tooltip, initialValue)
        {
        }

        protected override void DrawFieldValue(Rect rect)
        {
            SmartValue = EditorGUI.IntField(rect, SmartValue);
        }
    }

    public class BoolInputField : DialogInputField<bool>
    {
        public BoolInputField(string label, string tooltip = null, bool initialValue = default(bool))
            : base(label, tooltip, initialValue)
        {
        }

        protected override void DrawFieldValue(Rect rect)
        {
            SmartValue = EditorGUI.Toggle(rect, SmartValue);
        }
    }

    public class FloatInputField : DialogInputField<float>
    {
        public FloatInputField(string label, string tooltip = null, float initialValue = default(float))
            : base(label, tooltip, initialValue)
        {
        }


        protected override void DrawFieldValue(Rect rect)
        {
            SmartValue = EditorGUI.FloatField(rect, SmartValue);
        }
    }
}