using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityProperty : SimpleDrawable
    {
        public DrawableUnityProperty(SerializedProperty prop)
            : base(prop)
        {
        }
        
        protected override void Draw(object target)
        {
            var property = (SerializedProperty) target;
            EditorGUILayout.PropertyField(property, GUIContentHelper.TempContent(property.displayName));
        }

        protected override void Draw(Rect rect, object target)
        {
            var property = (SerializedProperty) target; 
            EditorGUI.PropertyField(rect, property, GUIContentHelper.TempContent(property.displayName));
        }
    }
}