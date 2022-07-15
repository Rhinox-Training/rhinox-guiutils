using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class DialogInputField
    {
        protected GUIContent _label;
        public GUIContent Label => _label;

        public abstract object WeakValue { get; }
        public float Height { get; protected set; } = EditorGUIUtility.singleLineHeight + 4;

        public delegate void InputFieldHandler(DialogInputField field);

#pragma warning disable 67

        public event InputFieldHandler ValidateValue;
        public event InputFieldHandler ValueChanged;

#pragma warning restore 67

        public void Draw(GUIContent label)
        {
            DrawField(label ?? Label);
        }

        public virtual float GetWidth()
        {
            return 0;
        }

        private void DrawField(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            if (label != null)
            {
#if ODIN_INSPECTOR
                var labelWidth = Sirenix.Utilities.Editor.GUIHelper.BetterLabelWidth;
#else
                var labelWidth = EditorGUIUtility.labelWidth;
#endif
                var labelRect = AlignLeft(rect, labelWidth);
                var fieldRect = AlignRight(rect, rect.width - labelWidth);

                EditorGUI.LabelField(labelRect, label);
                DrawFieldValue(fieldRect);
            }
            else
                DrawFieldValue(rect);
        }

        public static Rect AlignLeft(Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        public static Rect AlignRight(Rect rect, float width)
        {
            rect.x = rect.x + rect.width - width;
            rect.width = width;
            return rect;
        }

        protected abstract void DrawFieldValue(Rect rect);
    }

    public abstract class DialogInputField<T> : DialogInputField
    {
        public T SmartValue;

        public T Get() => SmartValue;

        protected DialogInputField(string label, string tooltip = null, T initialValue = default(T))
        {
            _label = new GUIContent(label, tooltip);
            SmartValue = initialValue;
        }

        public override object WeakValue => SmartValue;
    }
}