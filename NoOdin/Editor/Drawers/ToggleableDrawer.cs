using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(Toggleable<>))]
    public class ToggleableDrawer : PropertyDrawer
    {
        private const float _padding = 2;
        private const float _toggleWidth = 15;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = position.HorizontalPadding(_padding);
            var togglePos = position.AlignLeft(_toggleWidth);
            var itemPos = position.AlignRight(position.width - _toggleWidth - _padding);

            // Toggled Prop
            property.Next(true);
            EditorGUI.PropertyField(togglePos, property, GUIContent.none, false);

            GUIContentHelper.PushDisabled(!property.boolValue);

            // Item Prop
            property.Next(false);
            EditorGUI.PropertyField(itemPos, property, GUIContent.none, false);

            GUIContentHelper.PopDisabled();
        }
    }
}
