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

    public abstract class BaseEditor<T> : BaseEditor, IEditor, IRepaintRequest, IRepaintRequestHandler
        where T : class // UnityEngine.Object
    {
        public Type TargetType = typeof(T);
        protected SerializedProperty _monoscriptField;

        protected T Target => ConvertObject(target);
        protected T[] Targets => Array.ConvertAll(targets, ConvertObject);
        
        private bool _repaintRequested;
        private IRepaintRequest _repainter;
        
        //==============================================================================================================
        // EVENTS

        public event Action OnBeginGUI;
        public event Action OnEndGUI;
        public event Action OnClose;

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
            OnBeginGUI?.Invoke();

            OnInspectorGUI();
            RepaintIfRequested();
            
            OnEndGUI?.Invoke();
        }

        public void Destroy()
        {
            OnClose?.Invoke();
            Object.DestroyImmediate(this);
        }
        
        protected virtual IEditor CreateEditorForTarget(object obj)
        {
            if (obj is EditorWindow editorWindow)
                return TryCreateGenericEditor(editorWindow);

            if (obj is Object targetObject)
            {
                var curEditor = CreateStandardEditor(targetObject);
                if (curEditor == null)
                    curEditor = TryCreateGenericEditor(targetObject);

                return curEditor;
            }

            return TryCreateGenericNonUnityEditor(obj);
        }
        
        protected IEditor TryCreateGenericNonUnityEditor(object systemObj)
        {
            try
            {
                return GenericSmartObjectEditor.Create(systemObj);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        protected static IEditor CreateStandardEditor(UnityEngine.Object targetObject)
        {
            var editor = UnityEditor.Editor.CreateEditor(targetObject);
            // if (editor is MaterialEditor matEditor && s_materialForceVisibleProperty != null)
            //     s_materialForceVisibleProperty.SetValue(matEditor, true, null);
            if (editor != null)
                return new UnityEditorWrapper(editor);
            return null;
        }
        
        protected IEditor TryCreateGenericEditor(Object targetObject)
        {
            UnityEditor.Editor customEditor = null;
            try
            {
                customEditor = UnityEditor.Editor.CreateEditor(targetObject, typeof(GenericSmartUnityObjectEditor));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
            
            return new UnityEditorWrapper(customEditor);
        }
        
        public void RequestRepaint()
        {
            _repaintRequested = true;
        }

        protected void RepaintIfRequested()
        {
            if (!_repaintRequested)
                return;
            
            if (_repainter != null)
                _repainter.RequestRepaint();
            else
                Repaint();
            
            _repaintRequested = false;
        }
        
        public void UpdateRequestTarget(IRepaintRequest target)
        {
            _repainter = target;
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