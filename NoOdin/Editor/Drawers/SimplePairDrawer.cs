using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(SimplePair<,>))]
    public class SimplePairDrawer : PropertyDrawer
    {
        private const float _padding = 2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = position.HorizontalPadding(_padding);

            var v1Pos = position.AlignLeft(position.width / 2 - _padding);
            var v2Pos = position.AlignRight(position.width / 2 - _padding);

            // Toggled Prop
            property.Next(true);
            EditorGUI.PropertyField(v1Pos, property, GUIContent.none, false);

            // Item Prop
            property.Next(false);
            EditorGUI.PropertyField(v2Pos, property, GUIContent.none, false);
        }
    }
}