using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    [Serializable, IgnoreInScriptableObjectCreator]
    public class SmartInternalObject : ScriptableObject
    {
        [SerializeField]
        public object Target;
    }
    
    [UnityEditor.CustomEditor(typeof(SmartInternalObject))]
    public class GenericSmartObjectEditor : UnityEditor.Editor, IEditor, IRepaintRequestHandler
    {
        private DrawablePropertyView _propertyView;
        private MethodInfo[] _drawerMethods;
        private object _target;
        private IRepaintRequest _repainter;

        private void OnEnable()
        {
            var smartInternalObj = serializedObject.targetObject as SmartInternalObject;
            if (smartInternalObj == null)
                return;

            _target = smartInternalObj.Target;
            _drawerMethods = GetInspectorGUIMethods(_target);
            if (_target != null)
            {
                _propertyView = new DrawablePropertyView(_target);
                _propertyView.RepaintRequested += RequestRepaint;
            }
        }
        
        public bool CanDraw() => _target != null;
        public void Draw() => OnInspectorGUI();

        public override void OnInspectorGUI()
        {
            if (_propertyView != null)
                _propertyView.DrawLayout();
            
            foreach (var info in _drawerMethods)
                info.Invoke(_target, null);
        }
        
        private MethodInfo[] GetInspectorGUIMethods(object o)
        {
            if (o == null)
            {
                return Array.Empty<MethodInfo>();
            }

            var methods = o.GetType().GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttribute<OnInspectorGUIAttribute>() != null)
                .ToArray();
            return methods;
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

        public void Destroy()
        {
            Object.DestroyImmediate(this);
        }

        public void RequestRepaint()
        {
            if (_repainter != null)
                _repainter.RequestRepaint();
            else
                Repaint();
        }

        public void UpdateRequestTarget(IRepaintRequest target)
        {
            _repainter = target;
        }
    }
}