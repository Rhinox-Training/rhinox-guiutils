using System;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Base class for creating a scene overlay window (like navmesh)
    /// NOTE: It doesn't need to be an EditorWindow, I chose it that so it falls in line with other similar things
    /// </summary>
    public abstract class CustomSceneOverlayWindow<T> : EditorWindow where T : CustomSceneOverlayWindow<T>
    {
        protected bool _active;

        private static T _window;
        protected static T Window => _window != null ? _window : GetWindowInstance();

        protected virtual int Order => -1;

        private static T GetWindowInstance()
        {
            var objects = Resources.FindObjectsOfTypeAll<T>();
            if (!objects.IsNullOrEmpty())
                _window = objects[0];
            else
                _window = CreateInstance<T>();
            _window.Initialize();
            return _window;
        }

        protected abstract string Name { get; }

        protected virtual void Initialize()
        {
            titleContent = new GUIContent(Name);
        }

        protected virtual void Setup()
        {
            if (_active)
                UndockWindow();
            else
                DockWindow();
        }
        
        protected abstract string GetMenuPath();

        protected virtual bool HandleValidateWindow()
        {
            Menu.SetChecked(GetMenuPath(), _active);
            return IsActivatable(); // returns whether it is clickable
        }

        protected virtual bool IsActivatable()
        {
            return true;
        }

        public void DockWindow()
        {
            Utility.SubscribeToSceneGui(ShowSceneGUI);
            Selection.selectionChanged += OnSelectionChanged;
            _active = true;
            RepaintSceneAndGameViews();
        }

        public void UndockWindow()
        {
            Utility.UnsubscribeFromSceneGui(ShowSceneGUI);
            Selection.selectionChanged -= OnSelectionChanged;
            _active = false;
            RepaintSceneAndGameViews();
        }

        protected virtual void OnSelectionChanged()
        { }

        private void ShowSceneGUI(SceneView sceneView)
        {
            if (sceneView.camera == null) return;

            OnBeforeDraw();

            SceneOverlay.AddWindow(Name, HandleSceneGUI, Order);
            
            this.OnSceneGUI(sceneView);
        }

        private void HandleSceneGUI(Object target, SceneView sceneView)
        {
            this.OnGUI();
        }

        protected virtual void OnBeforeDraw()
        { }

        protected virtual void OnSceneGUI(SceneView sceneView)
        { }
        
        protected abstract void OnGUI();
        
        private static void RepaintSceneAndGameViews()
        {
            SceneView.RepaintAll();
            // PlayModeView.RepaintAll();
        }
    }
}