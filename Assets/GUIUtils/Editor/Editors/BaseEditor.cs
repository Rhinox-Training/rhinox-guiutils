﻿using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseEditor
#if ODIN_INSPECTOR
        : OdinEditor
#else
        : UnityEditor.Editor
#endif
    {
#if !ODIN_INSPECTOR // OdinEditor implements these, to allow easy override make stubs
        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }
#endif
    }

    public abstract class BaseEditor<T> : BaseEditor, IEditor
        where T : class // UnityEngine.Object
    {
        public Type TargetType = typeof(T);
        protected SerializedProperty _monoscriptField;

        protected T Target => ConvertObject(target);
        protected T[] Targets => Array.ConvertAll(targets, ConvertObject);

        protected void DrawScriptField()
        {
            if (_monoscriptField == null)
                _monoscriptField = serializedObject.FindProperty("m_Script");
            GUIContentHelper.PushDisabled(true);
            EditorGUILayout.PropertyField(_monoscriptField);
            GUIContentHelper.PopDisabled();
        }

        public bool CanDraw()
        {
            return Target != null;
        }

        public void Draw()
        {
            OnInspectorGUI();
        }

        public void Destroy()
        {
            Object.DestroyImmediate(this);
        }

        protected void Each(Action<T> update)
        {
            foreach (var t in Targets)
                update(t);
        }

        protected bool Any(Func<T, bool> check)
        {
            foreach (var t in Targets)
            {
                if (check(t))
                    return true;
            }

            return false;
        }

        protected bool All(Func<T, bool> check)
        {
            foreach (var t in Targets)
            {
                if (check(t) == false)
                    return false;
            }

            return true;
        }

        // Method to prevent lambda alloc
        protected virtual T ConvertObject(Object o) => o as T;
    }
    
}