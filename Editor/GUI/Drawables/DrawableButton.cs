﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableButton : BaseDrawable
    {
        protected override string LabelString => null; // TODO: Button has no label?
        
        public string Name { get; set; }
        
        public float Height { get; set; }

        private readonly MethodInfo _methodInfo;

        public DrawableButton(object instanceVal, MethodInfo method)
        {
            HostInfo = new GenericHostInfo(instanceVal, method);
            _methodInfo = method;
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (Name.IsNullOrEmpty())
                Name = _methodInfo?.GetNiceName();
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            var height = Height == 0 ? (int)EditorGUIUtility.singleLineHeight : Height;
            if (GUILayout.Button(Name, options.Append(GUILayout.Height(height))))
            {
                var host = HostInfo.GetHost();
                if (!TryCreateDialog(_methodInfo, host))
                    _methodInfo.Invoke(host, null);
            }
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var buttonHeight = Height == 0 ? (int)EditorGUIUtility.singleLineHeight : Height;
            rect.height = buttonHeight;
            if (GUI.Button(rect, Name))
            {
                if (!TryCreateDialog(_methodInfo, HostInfo))
                    _methodInfo.Invoke(HostInfo, null);
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
                    dialog = dialog.IntField(paramInfo.GetNiceName(), out var intField);
                    valueReferences.AddUnique(intField);
                }
                else if (paramInfo.ParameterType == typeof(bool))
                {
                    dialog = dialog.BooleanField(paramInfo.GetNiceName(), out var boolField);
                    valueReferences.AddUnique(boolField);
                }
                else if (paramInfo.ParameterType == typeof(float))
                {
                    dialog = dialog.FloatField(paramInfo.GetNiceName(), out var floatField);
                    valueReferences.AddUnique(floatField);
                }
                else if (paramInfo.ParameterType == typeof(string))
                {
                    dialog = dialog.TextField(paramInfo.GetNiceName(), out var stringField);
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