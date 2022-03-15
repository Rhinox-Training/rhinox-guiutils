using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class AnimationCurveHeightAttributeDrawer : OdinAttributeDrawer<AnimationCurveHeightAttribute, AnimationCurve>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(true, this.Attribute.height);

            var animationCurve = this.ValueEntry.SmartValue;

            if (label == null)
                animationCurve = EditorGUI.CurveField(rect, animationCurve);
            else 
                animationCurve = EditorGUI.CurveField(rect, label, animationCurve);

            this.ValueEntry.SmartValue = animationCurve;
        }
    }
}
