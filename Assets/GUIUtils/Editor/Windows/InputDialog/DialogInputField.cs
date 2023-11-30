using System;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class DialogInputField
    {
        protected string _tooltip;
        
        protected GUIContent _label;
        public GUIContent Label => _label;
        
        public abstract object WeakValue { get; }
        public float Height { get; protected set; } = EditorGUIUtility.singleLineHeight + 4;
        public virtual bool IsDataValid => true;

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
            if (label != null && !label.text.IsNullOrEmpty())
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
            {
                DrawFieldValue(rect);
            }
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

        public override object WeakValue => SmartValue;

        public override bool IsDataValid
        {
            get
            {
                if (_customValidator == null)
                    return base.IsDataValid;
                return _customValidator.Invoke(SmartValue);
            }
        }

        protected Func<T, bool> _customValidator;

        protected DialogInputField(string label, string tooltip = null, T initialValue = default(T))
        {
            _tooltip = tooltip;
            _label = new GUIContent(label, tooltip);
            SmartValue = initialValue;
        }

        public void SetValidator(Func<T, bool> validationFunction)
        {
            _customValidator = validationFunction;
        }
    }
}