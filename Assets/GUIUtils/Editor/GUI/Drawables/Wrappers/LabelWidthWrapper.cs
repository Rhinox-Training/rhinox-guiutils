using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class LabelWidthWrapper : BaseWrapperDrawable
    {
        public float _width;
        private float _originalLabelWidth;

        public LabelWidthWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            _originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = _width;
            base.OnPreDraw();
        }

        protected override void OnPostDraw()
        {
            base.OnPostDraw();
            EditorGUIUtility.labelWidth = _originalLabelWidth;
        }


        [WrapDrawer(typeof(LabelWidthAttribute), -10000)]
        public static BaseWrapperDrawable Create(LabelWidthAttribute attr, IOrderedDrawable drawable)
        {
            return new LabelWidthWrapper(drawable)
            {
                _width = attr.Width
            };
        }
    }
}