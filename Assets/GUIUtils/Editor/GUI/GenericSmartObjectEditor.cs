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
        private object _target;
        private IRepaintRequest _repainter;

        private void OnEnable()
        {
            if (target == null)
                return;
            
            var smartInternalObj = serializedObject.targetObject as SmartInternalObject;
            if (smartInternalObj == null)
                return;

            _target = smartInternalObj.Target;
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