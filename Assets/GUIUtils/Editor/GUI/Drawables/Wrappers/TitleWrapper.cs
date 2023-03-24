using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TitleWrapper : WrapperDrawable
    {
        public string _title;
        public bool _bold;

        public TitleWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label)
        {
            TryDrawTitle();
            base.DrawInner(label);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            TryDrawTitle();
            base.DrawInner(rect, label);
        }

        private void TryDrawTitle()
        {
            if (!string.IsNullOrEmpty(Title))
            {
                EditorGUILayout.LabelField(Title, _bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title);
                CustomEditorGUI.HorizontalLine(CustomGUIStyles.LightBorderColor, 1.5f);
            }
        }

        [WrapDrawer(typeof(TitleAttribute))]
        public static WrapperDrawable Create(TitleAttribute attr, IOrderedDrawable drawable)
        {
            return new TitleWrapper(drawable)
            {
                _title = attr.Title,
                _bold = attr.Bold
            };
        }
    }
}