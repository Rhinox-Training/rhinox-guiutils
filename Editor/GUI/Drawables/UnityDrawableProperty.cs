using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityDrawableProperty : SimpleDrawable
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
    }
}