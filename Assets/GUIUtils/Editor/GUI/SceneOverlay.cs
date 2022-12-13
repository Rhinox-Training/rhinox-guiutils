using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// This used the 'SceneViewOverlay' which is an internal unity class
    /// This may (will) break in a future update
    /// </summary>
    public static class SceneOverlay
    {
        public enum WindowDisplayOption
        {
            MultipleWindowsPerTarget,
            OneWindowPerTarget,
            OneWindowPerTitle,
        }

        public delegate void WindowFunction(Object target, SceneView sceneView);

        private static Assembly _editorAssembly;

        private static Assembly EditorAssembly =>
            _editorAssembly ?? (_editorAssembly = Assembly.GetAssembly(typeof(EditorWindow)));

        public static object AddWindow(string title, WindowFunction sceneViewFunc, int order = -1,
            WindowDisplayOption option = WindowDisplayOption.OneWindowPerTitle)
        {
            return OpenWindow(new GUIContent(title), sceneViewFunc, order, option);
        }

        public static object AddWindow(GUIContent title, WindowFunction sceneViewFunc, int order = -1,
            WindowDisplayOption option = WindowDisplayOption.OneWindowPerTitle)
        {
            return OpenWindow(title, sceneViewFunc, order, option);
        }

        private static object OpenWindow(GUIContent title, WindowFunction sceneViewFunc, int order,
            WindowDisplayOption option)
        {
            var t = EditorAssembly.GetType("UnityEditor.SceneViewOverlay");

            var mi = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).Single(
                m =>
                    m.Name == "Window"
                    && m.GetParameters().Length == 4
            );

            var delegateT = mi.GetParameters()[1].ParameterType;

            var castedDelegate = DelegateUtility.Cast(sceneViewFunc, delegateT);

            var o = mi.Invoke(null, new object[] { title, castedDelegate, order, (int)option });

            return o;
        }
    }
}