using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rhinox.GUIUtils.UnitTesting
{
    [ExecuteAlways]
    public class OnGUITestWrapper : MonoBehaviour, IMonoBehaviourTest
    {
        public bool bullshit;
        
        [MenuItem("CONTEXT/Transform/Add ONGUI")]
        private static void Foboar(MenuCommand command)
        {
            var t = command.context as Transform;
            t.gameObject.AddComponent<OnGUITestWrapper>();
        }
        
        public PropertyInfo Property { get; set; }
        private bool _isDone = false;
        private static EditorWindow _window;

        public bool IsTestFinished
        {
            get { return _isDone; }
        }
        
        private static void FocusGameTab()
        {
            if (_window == null)
            {
                System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
                System.Type type = assembly.GetType("UnityEditor.GameView");
                _window = EditorWindow.GetWindow(type);
            }
            _window.Focus();
        }

        void Awake()
        {
            FocusGameTab();
        }

        void OnGUI()
        {
            try
            {
                if (Property != null)
                {
                    var result = Property.GetMethod.Invoke(null, null);
                    Assert.IsNotNull(result);
                }
                else
                {
                    Assert.Fail();
                }
            }
            catch (Exception e)
            {
                _isDone = true;
                throw e;
            }
            finally
            {
                _isDone = true;
            }
        }
    }
}