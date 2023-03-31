using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class LabelWidthWrapper : BaseWrapperDrawable
    {
        private float _width;
        private bool _fitToLabel;
        
        public LabelWidthWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        private float HandleLabelWidth(GUIContent label)
        {
            float original = EditorGUIUtility.labelWidth;

            float width = _width;

            if (_fitToLabel)
            {
                GUI.skin.label.CalcMinMaxWidth(label, out float min, out float max);
                width = Mathf.Max(max, width);
            }
            
            EditorGUIUtility.labelWidth = width;

            return original;
        } 

        protected override void DrawInner(GUIContent label)
        {
            float restoreValue = HandleLabelWidth(label);
            
            base.DrawInner(label);

            EditorGUIUtility.labelWidth = restoreValue;
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            float restoreValue = HandleLabelWidth(label);

            base.DrawInner(rect, label);
            
            EditorGUIUtility.labelWidth = restoreValue;
        }


        [WrapDrawer(typeof(LabelWidthAttribute), -500)]
        public static BaseWrapperDrawable Create(LabelWidthAttribute attr, IOrderedDrawable drawable)
        {
            return new LabelWidthWrapper(drawable)
            {
                _width = attr.Width
            };
        }
        
        [WrapDrawer(typeof(FittedLabelAttribute), -500)]
        public static BaseWrapperDrawable Create(FittedLabelAttribute attr, IOrderedDrawable drawable)
        {
            return new LabelWidthWrapper(drawable)
            {
                _fitToLabel = true
            };
        }
    }
}