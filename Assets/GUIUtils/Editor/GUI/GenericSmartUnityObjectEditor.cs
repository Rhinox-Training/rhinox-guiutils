using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rhinox.GUIUtils.Editor
{
    [UnityEditor.CustomEditor(typeof(object))]
    public class GenericSmartUnityObjectEditor : UnityEditor.Editor
    {
        private ICollection<IOrderedDrawable> _drawables;

        private void OnEnable()
        {
            _drawables = DrawableFactory.ParseSerializedObject(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw();
            }
        }
        
        // TODO: utils?/lightspeed?
        // public static object GetValue(SerializedProperty property)
        // {
        //     System.Type parentType = property.serializedObject.targetObject.GetType();
        //     System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);  
        //     return fi.GetValue(property.serializedObject.targetObject);
        // }
        // public static void SetValue(SerializedProperty property,object value)
        // {
        //     System.Type parentType = property.serializedObject.targetObject.GetType();
        //     System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);//this FieldInfo contains the type.
        //     fi.SetValue(property.serializedObject.targetObject, value);
        // }
    }
}