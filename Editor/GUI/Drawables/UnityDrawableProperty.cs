using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityDrawableProperty : UnitySerializedDrawable
    {
        private readonly SerializedProperty _property;

        public UnityDrawableProperty(SerializedProperty prop)
            : base(prop.serializedObject)
        {
            _property = prop;
        }
        
        protected override void Draw(UnityEngine.Object target)
        {
            EditorGUILayout.PropertyField(_property, GUIContentHelper.TempContent(_property.displayName));
        }

        protected override void Draw(Rect rect, Object target)
        {
            EditorGUI.PropertyField(rect, _property, GUIContentHelper.TempContent(_property.displayName));
        }
    }
}