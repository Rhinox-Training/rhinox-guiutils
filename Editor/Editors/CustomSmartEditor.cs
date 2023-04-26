﻿using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class CustomSmartEditor : BaseEditor<MonoBehaviour>, IRepaintRequestHandler
    {
        private DrawablePropertyView _propertyView;
        private IRepaintRequest _target;
        
        public override void OnInspectorGUI()
        {
            var attr = target.GetType().GetCustomAttribute<SmartFallbackDrawnAttribute>();
            if (attr == null)
            {
                base.OnInspectorGUI();
                return;
            }

            if (attr.AllowUnityIfAble)
            {
                int count = CountDrawnProperties(serializedObject);
                if (count > 0)
                {
                    base.OnInspectorGUI();
                    return;
                }
            }

            if (_propertyView == null)
            {
                _propertyView = new DrawablePropertyView(serializedObject);
                _propertyView.RepaintRequested += OnRepaintRequested;
            }
            
            DrawScriptField();
            _propertyView.DrawLayout();
        }

        private void OnRepaintRequested()
        {
            if (_target != null)
                _target.RequestRepaint();
            else
                Repaint();
        }

        private static int CountDrawnProperties(SerializedObject obj)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            int count = 0;
            while (iterator.NextVisible(enterChildren))
            {
                count++;
                enterChildren = false;
            }

            return count;
        }

        public void UpdateRequestTarget(IRepaintRequest target)
        {
            _target = target;
        }
    }
}