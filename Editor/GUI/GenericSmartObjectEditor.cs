using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [Serializable]
    public class SmartInternalObject : ScriptableObject
    {
        [SerializeField]
        public object Target;
    }
    
    [UnityEditor.CustomEditor(typeof(SmartInternalObject))]
    public class GenericSmartObjectEditor : UnityEditor.Editor
    {
        private DrawablePropertyView _propertyView;

        private void OnEnable()
        {
            if (_propertyView == null)
                _propertyView = new DrawablePropertyView((serializedObject.targetObject as SmartInternalObject).Target);
        }

        public override void OnInspectorGUI()
        {
            _propertyView.DrawLayout();
        }

        public static GenericSmartObjectEditor Create(object systemObj)
        {
            GenericSmartObjectEditor customEditor = null;
            try
            {
                var scriptableTarget = ScriptableObject.CreateInstance<SmartInternalObject>();
                scriptableTarget.Target = systemObj;
                
                customEditor = UnityEditor.Editor.CreateEditor(scriptableTarget, typeof(GenericSmartObjectEditor)) as GenericSmartObjectEditor;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                customEditor = null;
            }

            return customEditor;
        }
    }
}