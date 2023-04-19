using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityProperty : BaseEntityDrawable<SerializedProperty>
    {
        private HostInfo _info;
        public override string LabelString => Entity != null ? Entity.displayName : base.LabelString;

        public DrawableUnityProperty(SerializedProperty prop)
            : base(prop, prop.FindFieldInfo())
        {
            Host = prop;
            _info = prop.GetHostInfo();
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options) 
        {
            EditorGUILayout.PropertyField(Entity, label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.PropertyField(rect, Entity, label);
        }

        public override object GetValue()
        {
            return _info.GetValue();
        }

        public override bool TrySetValue(object value)
        {
            _info.SetValue(value);
            return true;
        }
    }
}