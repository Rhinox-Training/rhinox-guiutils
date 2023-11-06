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
            GUILayout.BeginHorizontal(CustomGUIStyles.Clean);
            base.DrawInner(GUIContent.none, options.Append(GUILayout.MaxWidth(_toggleWidth))); 
            GUILayout.Label(label);
            GUILayout.EndHorizontal();
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

        [WrapDrawer(typeof(ToggleLeftAttribute), Priority.Simple)]
        public static BaseWrapperDrawable Create(ToggleLeftAttribute attr, IOrderedDrawable drawable)
        {
            return new ToggleLeftWrapper(drawable)
            {
            };
        }
    }
}