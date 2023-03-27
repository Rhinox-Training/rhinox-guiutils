using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [Serializable, IgnoreInScriptableObjectCreator]
    public class SmartInternalObject : ScriptableObject
    {
        [SerializeField]
        public object Target;
    }
    
    [UnityEditor.CustomEditor(typeof(SmartInternalObject))]
    public class GenericSmartObjectEditor : UnityEditor.Editor
    {
        private DrawablePropertyView _propertyView;
        private MethodInfo _drawerMethod;
        private object _target;

        private void OnEnable()
        {
            var smartInternalObj = serializedObject.targetObject as SmartInternalObject;
            if (smartInternalObj == null)
                return;

            _target = smartInternalObj.Target;
            if (TryGetInspectorGUIMethod(_target, out MethodInfo methodInfo))
                _drawerMethod = methodInfo;
            if (_target != null)
                _propertyView = new DrawablePropertyView(_target);
        }

        public override void OnInspectorGUI()
        {
            if (_propertyView != null)
                _propertyView.DrawLayout();
            if (_drawerMethod != null)
                _drawerMethod.Invoke(_target, null);
        }
        
        private bool TryGetInspectorGUIMethod(object o, out MethodInfo methodInfo)
        {
            if (o == null)
            {
                methodInfo = null;
                return false;
            }

            var methods = o.GetType().GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttribute<OnInspectorGUIAttribute>() != null)
                .ToArray();
            methodInfo = methods.FirstOrDefault();
            return methodInfo != null;
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