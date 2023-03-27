using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public interface ICustomSceneOverlayWindow
    {
        bool IsActive { get; }
        void DockWindow();
        void UndockWindow();
    }

    /// <summary>
    /// Base class for creating a scene overlay window (like navmesh)
    /// NOTE: It doesn't need to be an EditorWindow, I chose it that so it falls in line with other similar things
    /// </summary>
    public abstract class CustomSceneOverlayWindow<T> : ICustomSceneOverlayWindow where T : CustomSceneOverlayWindow<T>, new()
    {
        protected PersistentValue<bool> _active;

        public bool IsActive => _active;

        private static T _window;
        protected static T Window => _window != null ? _window : GetWindowInstance();

        protected virtual int Order => -1;

        private static T GetWindowInstance()
        {
            if (_window == null)
                _window = new T();
            _window.Initialize();
            return _window;
        }

        protected abstract string Name { get; }

        protected virtual void Initialize()
        {
            _active = PersistentValue<bool>.Create(typeof(T), nameof(_active), false);
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
            _active.Set(true);
            RepaintSceneAndGameViews();
        }

        public void UndockWindow()
        {
            Utility.UnsubscribeFromSceneGui(ShowSceneGUI);
            Selection.selectionChanged -= OnSelectionChanged;
            _active.Set(false);
            
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
        }
    }
    
    internal static class OverlayWindowResurrector
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnReloadScripts()
        {
            // Is Unity still compiling or reloading assets
            if(EditorApplication.isCompiling || EditorApplication.isUpdating)
                AssemblyReloadEvents.afterAssemblyReload += RecreateOverlayWindowsIfNeeded; // Delay some more
            else
                EditorApplication.delayCall += RecreateOverlayWindowsIfNeeded;
        }

        private static void RecreateOverlayWindowsIfNeeded()
        {
            AssemblyReloadEvents.afterAssemblyReload -= RecreateOverlayWindowsIfNeeded;

            var types = AppDomain.CurrentDomain.GetDefinedTypesOfType<ICustomSceneOverlayWindow>();

            foreach (var type in types)
            {
                var supertype = typeof(CustomSceneOverlayWindow<>).MakeGenericType(type);
                var prop = supertype.GetProperty("Window",
                    BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);

                var window = prop.GetGetMethod(true).Invoke(null, null) as ICustomSceneOverlayWindow;
                
                if (window.IsActive)
                    window.DockWindow();
            }
        }
    }
}