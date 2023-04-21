using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableButton : BaseDrawable
    {
        public override string LabelString => null; // TODO: Button has no label?
        
        public string Name { get; set; }
        
        public float Height { get; set; }

        private readonly MethodInfo _methodInfo;

        public DrawableButton(object instanceVal, MethodInfo method)
        {
            Host = instanceVal;
            _methodInfo = method;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            var height = Height == 0 ? (int)EditorGUIUtility.singleLineHeight : Height;
            if (GUILayout.Button((Name ?? _methodInfo.Name).SplitCamelCase(), options.Append(GUILayout.Height(height))))
            {
                if (!TryCreateDialog(_methodInfo, Host))
                    _methodInfo.Invoke(Host, null);
            }
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var buttonHeight = Height == 0 ? (int)EditorGUIUtility.singleLineHeight : Height;
            rect.height = buttonHeight;
            if (GUI.Button(rect, (Name ?? _methodInfo.Name).SplitCamelCase()))
            {
                if (!TryCreateDialog(_methodInfo, Host))
                    _methodInfo.Invoke(Host, null);
            }
        }

        public bool TryCreateDialog(MethodInfo method, object instance)
        {
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
                return false;

            var dialog = EditorInputDialog.Create(method.Name, "Provide additional parameters:");
            var valueReferences = new List<DialogBuilder.IValueReference>();
            foreach (var paramInfo in parameters)
            {
                if (paramInfo.ParameterType == typeof(int))
                {
                    dialog = dialog.IntField(paramInfo.Name, out var intField);
                    valueReferences.AddUnique(intField);
                }
                else if (paramInfo.ParameterType == typeof(bool))
                {
                    dialog = dialog.BooleanField(paramInfo.Name, out var boolField);
                    valueReferences.AddUnique(boolField);
                }
                else if (paramInfo.ParameterType == typeof(float))
                {
                    dialog = dialog.FloatField(paramInfo.Name, out var floatField);
                    valueReferences.AddUnique(floatField);
                }
                else if (paramInfo.ParameterType == typeof(string))
                {
                    dialog = dialog.TextField(paramInfo.Name, out var stringField);
                    valueReferences.AddUnique(stringField);
                }
                else
                {
                    Debug.LogError($"Not supported {paramInfo.ParameterType.Name} for dialog builder");
                    return false;
                }
            }

            dialog.OnAccept(() =>
                {
                    var args = valueReferences.Select(x => x.GenericValue).ToArray();
                    method?.Invoke(instance, args);
                })
                .Show();
            return true;
        }

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_methodInfo == null)
                return base.GetDrawableAttributes<TAttribute>();
            return _methodInfo.GetCustomAttributes<TAttribute>();
        }
    }
}