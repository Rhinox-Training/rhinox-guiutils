using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DisplayAsStringWrapper : BaseWrapperDrawable
    {
        public override float ElementHeight
        {
            get
            {
                if (_drawable != null)
                    return EditorGUIUtility.singleLineHeight;
                return base.ElementHeight;
            }
        }
        
        public TextAlignment Alignment { get; private set; }
        public bool Overflow { get; private set; }

        private readonly IDrawableRead _drawable;
        private GUIStyle _currentLabelStyle;

        public DisplayAsStringWrapper(IOrderedDrawable drawable) : base(drawable)
        {
            if (drawable is IDrawableRead drawableRead)
                _drawable = drawableRead;
        } 
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            if (GetDisplayText(out GUIContent overrideText))
            {
                EditorGUILayout.LabelField(overrideText, options);
                return;
            }

            base.DrawInner(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (GetDisplayText(out GUIContent overrideText))
            {
                var style = GetStyle(Alignment);
                EditorGUI.LabelField(rect, overrideText, style);
                return;
            }

            base.DrawInner(rect, label);
        }

        private GUIStyle GetStyle(TextAlignment alignment)
        {
            if (_currentLabelStyle == null)
            {
                GUIStyle labelStyle = null;
                switch (alignment)
                {
                    case TextAlignment.Center:
                        labelStyle = CustomGUIStyles.CenteredLabel;
                        break;
                    case TextAlignment.Right:
                        labelStyle = CustomGUIStyles.LabelRight;
                        break;
                    default:
                        labelStyle = CustomGUIStyles.Label;
                        break;
                }

                if (labelStyle != null)
                {
                    labelStyle = new GUIStyle(labelStyle);
                    labelStyle.wordWrap = !Overflow;
                }

                _currentLabelStyle = labelStyle;
            }

            return _currentLabelStyle;
        }

        private bool GetDisplayText(out GUIContent contentLabel)
        {
            if (_drawable == null)
            {
                contentLabel = null;
                return false;
            }
            
            object value = _drawable.GetValue();
            
            if (value is SerializedProperty serializedProperty)
                value = serializedProperty.GetValue();

            if (value != null)
                contentLabel = GUIContentHelper.TempContent(value.ToString());
            else
                contentLabel = GUIContent.none;
            
            return true;
        }

        [WrapDrawer(typeof(DisplayAsStringAttribute), Priority.Important)]
        public static BaseWrapperDrawable Create(DisplayAsStringAttribute attr, IOrderedDrawable drawable)
        {
            return new DisplayAsStringWrapper(drawable)
            {
                Alignment = TextAlignment.Left,
                Overflow = attr.Overflow
            };
        }

        [WrapDrawer(typeof(DisplayAsStringAlignedAttribute), Priority.Important)]
        public static BaseWrapperDrawable Create(DisplayAsStringAlignedAttribute attr, IOrderedDrawable drawable)
        {
            return new DisplayAsStringWrapper(drawable)
            {
                Alignment = attr.Alignment,
                Overflow = attr.Overflow
            };
        }
    }
}