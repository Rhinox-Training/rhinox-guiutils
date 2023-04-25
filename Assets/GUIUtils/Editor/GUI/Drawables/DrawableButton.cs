using System;
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

        public new MethodHostInfo HostInfo => (MethodHostInfo) _hostInfo;
        
        public DrawableButton(GenericHostInfo info, MethodInfo method)
        {
            _hostInfo = new MethodHostInfo(info, method);
        }

        public DrawableButton(object instanceVal, MethodInfo method)
        {
            _hostInfo = new MethodHostInfo(instanceVal, method);
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (Name.IsNullOrEmpty())
                Name = _hostInfo.NiceName;
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            var height = Height == 0 ? (int)EditorGUIUtility.singleLineHeight : Height;
            if (GUILayout.Button(Name, options.Append(GUILayout.Height(height))))
                Invoke();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            var buttonHeight = Height == 0 ? (int)EditorGUIUtility.singleLineHeight : Height;
            rect.height = buttonHeight;
            if (GUI.Button(rect, Name))
                Invoke();
        }

        protected virtual void Invoke()
        {
            if (!TryCreateDialog())
                HostInfo.Invoke();
        }

        public bool TryCreateDialog()
        {
            var parameters = HostInfo.GetParameters();

            if (parameters.Length == 0)
                return false;

            var dialog = EditorInputDialog.Create(HostInfo.NiceName, "Provide additional parameters:");
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
                    HostInfo.Invoke(args);
                })
                .Show();
            return true;
        }

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            return HostInfo.MemberInfo.GetCustomAttributes<TAttribute>();
        }
    }
}