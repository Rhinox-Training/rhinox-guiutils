using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityProperty : BaseEntityDrawable<SerializedProperty>
    {
        public override string LabelString => Entity != null ? Entity.displayName : base.LabelString;

        public DrawableUnityProperty(SerializedProperty prop, MemberInfo memberInfo = null)
            : base(prop, memberInfo)
        {
            Host = prop;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options) 
        {
            EditorGUILayout.PropertyField(Entity, label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.PropertyField(rect, Entity, label);
        }
    }
}