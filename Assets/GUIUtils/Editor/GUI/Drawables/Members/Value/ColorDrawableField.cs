using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ColorDrawableField : BaseMemberValueDrawable<Color>
    {
        public ColorDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
        }

        protected override Color DrawValue(GUIContent label, Color val, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ColorField(label, val, options);
        }

        protected override Color DrawValue(Rect rect, GUIContent label, Color val)
        {
            return EditorGUI.ColorField(rect, label, val);
        }
    }
}