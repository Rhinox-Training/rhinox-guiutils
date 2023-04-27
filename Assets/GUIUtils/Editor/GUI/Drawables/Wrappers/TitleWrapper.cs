using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TitleWrapper : BaseWrapperDrawable
    {
        public string _title;
        public bool _bold;
        public TitleAlignments _alignment;

        private GUIStyle _style;

        public override float ElementHeight => base.ElementHeight + EditorGUIUtility.singleLineHeight + 4.0f;


        public TitleWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            _style = _bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title;

            if (_alignment == TitleAlignments.Left) return;
            
            _style = new GUIStyle(_style);

            if (_alignment == TitleAlignments.Right)
                _style.alignment = TextAnchor.MiddleRight;
            else if (_alignment == TitleAlignments.Centered)
                _style.alignment = TextAnchor.MiddleCenter;
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            if (!string.IsNullOrEmpty(_title))
            {
                GUILayout.Label(_title, _bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title);
                CustomEditorGUI.HorizontalLine(CustomGUIStyles.LightBorderColor, thickness: 1);
                GUILayout.Space(3.0f);
            }
            base.DrawInner(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (!string.IsNullOrEmpty(_title))
            {
                var labelRect = rect.AlignTop(EditorGUIUtility.singleLineHeight);
                rect.y += labelRect.height;
                rect.height -= labelRect.height;
                EditorGUI.LabelField(labelRect, _title, _bold ? CustomGUIStyles.BoldTitle : CustomGUIStyles.Title);
                var lineRect = rect.AlignTop(1.0f);
                CustomEditorGUI.HorizontalLine(lineRect, CustomGUIStyles.LightBorderColor, 1);
                rect.y += 4.0f;
                rect.height -= 4.0f;
            }
            base.DrawInner(rect, label);
        }

        [WrapDrawer(typeof(TitleAttribute), -10000)]
        public static BaseWrapperDrawable Create(TitleAttribute attr, IOrderedDrawable drawable)
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