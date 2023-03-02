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
    [UnityEditor.CustomEditor(typeof(UnityEngine.Object))]
    public class GenericSmartUnityObjectEditor : UnityEditor.Editor
    {
        private DrawablePropertyView _propertyView;

        private void OnEnable()
        {
            if (_propertyView == null)
                _propertyView = new DrawablePropertyView(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            _propertyView.DrawLayout();
        }
    }
}