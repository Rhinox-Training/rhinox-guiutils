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
    }
}