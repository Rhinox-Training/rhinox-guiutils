using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityProperty : BaseMemberDrawable, IDrawableReadWrite
    {
        protected override string LabelString => Property != null ? Property.displayName : base.LabelString;

        public override float ElementHeight 
        {
            get
            {
                if (Property == null)
                    return base.ElementHeight;
                
                float propertyHeight = EditorGUI.GetPropertyHeight(Property);
                return propertyHeight;
            }
        }

        public SerializedProperty Property { get; }
        
        public DrawableUnityProperty(SerializedProperty prop)
            : base(prop.GetHostInfo())
        {
            Property = prop;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options) 
        {
            EditorGUILayout.PropertyField(Property, label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.PropertyField(rect, Property, label);
        }

        protected override void OnPostDraw()
        {
            base.OnPostDraw();
            Property.serializedObject.ApplyModifiedProperties();
        }

        public object GetValue()
        {
            return HostInfo.GetValue();
        }

        public bool TrySetValue(object value)
        {
            return HostInfo.TrySetValue(value);
        }
    }
}