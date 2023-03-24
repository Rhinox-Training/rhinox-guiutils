using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TitleWrapper : WrapperDrawable
    {
        public string _title;
        public bool _bold;
        public TitleAlignments _alignment;

        private GUIStyle _style;

        public TitleWrapper(IOrderedDrawable drawable) : base(drawable)
        {
            _style = _bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title;

            if (_alignment == TitleAlignments.Left) return;
            
            _style = new GUIStyle(_style);

            if (_alignment == TitleAlignments.Right)
                _style.alignment = TextAnchor.MiddleRight;
            else if (_alignment == TitleAlignments.Centered)
                _style.alignment = TextAnchor.MiddleCenter;
        }

        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            
            TryDrawTitle();
        }

        private void TryDrawTitle()
        {
            if (string.IsNullOrEmpty(_title)) return;
            
            EditorGUILayout.LabelField(_title, _bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title);
            CustomEditorGUI.HorizontalLine(CustomGUIStyles.LightBorderColor, thickness: 1);
            EditorGUILayout.Space(3.0f);
        }

        [WrapDrawer(typeof(TitleAttribute), -10000)]
        public static WrapperDrawable Create(TitleAttribute attr, IOrderedDrawable drawable)
        {
            return new TitleWrapper(drawable)
            {
                _title = attr.Title,
                _bold = attr.Bold,
                _alignment = attr.TitleAlignment
            };
        }
    }
}