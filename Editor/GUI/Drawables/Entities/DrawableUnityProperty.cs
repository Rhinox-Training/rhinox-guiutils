using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityProperty : BaseEntityDrawable
    {
        public override string LabelString => _targetObj != null ? ((SerializedProperty) _targetObj).displayName : null;

        public DrawableUnityProperty(SerializedProperty prop, MemberInfo memberInfo = null)
            : base(prop, memberInfo)
        {
        }
        
        protected override void Draw(object target)
        {
            var property = (SerializedProperty) target;
            EditorGUILayout.PropertyField(property, Label);
        }

        protected override void Draw(Rect rect, object target)
        {
            var property = (SerializedProperty) target; 
            EditorGUI.PropertyField(rect, property, Label);
        }
    }
}