using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.GetReturnType() != typeof(int))
            {
                base.OnGUI(position, property, label);
                return;
            }
            
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}