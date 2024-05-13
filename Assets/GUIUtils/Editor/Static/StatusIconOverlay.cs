using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public static class StatusIconOverlay
    {
        private static readonly Type _toolbarType;
        private static readonly PropertyInfo _guiBackend;
        private static readonly PropertyInfo _visualTree;
        private static readonly FieldInfo _onGuiHandler;
        
        private static GUIStyle _iconStyle;
        private static Object _appStatusBar;
        private static VisualElement _container;

        private static IList<object> _activeItems;

        static StatusIconOverlay()
        {
            var editorAssembly = typeof(UnityEditor.Editor).Assembly;
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            _toolbarType = editorAssembly.GetType("UnityEditor.AppStatusBar");
            var guiViewType = editorAssembly.GetType("UnityEditor.GUIView");
            var backendType = editorAssembly.GetType("UnityEditor.IWindowBackend");
            var containerType = typeof(IMGUIContainer);
            
            _guiBackend = guiViewType?.GetProperty("windowBackend", bindingFlags);
            _visualTree = backendType?.GetProperty("visualTree", bindingFlags);
            _onGuiHandler = containerType?.GetField("m_OnGUIHandler", bindingFlags);
            
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (_appStatusBar == null)
            {
                Refresh();
            }
        }

        private static void Refresh()
        {
            var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
            if (toolbars == null || toolbars.Length == 0)
            {
                return;
            }

            _appStatusBar = toolbars[0];

            var backend = _guiBackend?.GetValue(_appStatusBar);
            if (backend == null)
            {
                return;
            }

            var elements = _visualTree?.GetValue(backend, null) as VisualElement;
            _container = elements?[0];
            if (_container == null)
            {
                return;
            }

            var handler = _onGuiHandler?.GetValue(_container) as Action;
            if (handler == null)
            {
                return;
            }

            handler -= RefreshGUI;
            handler += RefreshGUI;
            _onGuiHandler.SetValue(_container, handler);
        }
        
        private static void RefreshStyles()
        {
            if (_iconStyle != null)
                return;

            _iconStyle = new GUIStyle("StatusBarIcon");
        }
        
        private static void RefreshGUI()
        {
            if (_activeItems.IsNullOrEmpty())
                return;

            RefreshStyles();

            float currentPosition = _container.layout.width;
            currentPosition -= 160;
            // if oculus is not active, we could use 130
            foreach (var icon in _activeItems)
            {
                // Hardcoded position
                // Currently overlaps with progress bar, and works with 2020 status bar icons
                // TODO: Better hook to dynamically position the button
                var currentRect = new Rect(currentPosition, 0, 26, 30); // Hardcoded position
                GUILayout.BeginArea(currentRect);
                // if (GUILayout.Button(icon.Icon, _iconStyle))
                // {
                //     OVRStatusMenu.ShowDropdown(GUIUtility.GUIToScreenPoint(Vector2.zero));
                // }

                var buttonRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
                GUILayout.EndArea();
                
                currentPosition -= 30;
            }
        }
    }
}