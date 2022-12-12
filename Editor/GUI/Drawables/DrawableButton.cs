using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableButton : SimpleDrawable
    {
        public string Name { get; }

        private readonly MethodInfo _methodInfo;
        private readonly ButtonAttribute _buttonAttr;

        public DrawableButton(SerializedObject obj, MethodInfo method, ButtonAttribute buttonAttr)
            : base(obj)
        {
            _methodInfo = method;
            _buttonAttr = buttonAttr;
        }
        
        protected override void Draw(UnityEngine.Object target)
        {
            var buttonHeight = _buttonAttr.ButtonHeight;
            buttonHeight = buttonHeight == 0 ? (int)EditorGUIUtility.singleLineHeight : buttonHeight;
            if (GUILayout.Button(_buttonAttr.Name ?? _methodInfo.Name, GUILayout.Height(buttonHeight)))
            {
                if (!TryCreateDialog(_methodInfo, target))
                    _methodInfo.Invoke(target, null);
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
    }
}