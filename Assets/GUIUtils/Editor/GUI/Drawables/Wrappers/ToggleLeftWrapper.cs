using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ToggleLeftWrapper : BaseWrapperDrawable
    {
        private readonly float _toggleWidth = EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;
        
        public ToggleLeftWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal();
            base.DrawInner(GUIContent.none, options.Append(GUILayout.MaxWidth(_toggleWidth)));
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var toggleRect = rect.AlignLeft(_toggleWidth);
            base.DrawInner(toggleRect, GUIContent.none);
            var labelRect = rect;
            labelRect.width -= toggleRect.width;
            labelRect.x += toggleRect.width;
            EditorGUI.LabelField(labelRect, label);
        }

        [WrapDrawer(typeof(ToggleLeftAttribute))]
        public static BaseWrapperDrawable Create(ToggleLeftAttribute attr, IOrderedDrawable drawable)
        {
            return new ToggleLeftWrapper(drawable)
            {
            };
        }
    }
}