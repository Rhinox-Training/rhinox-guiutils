using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableMethod : BaseDrawable
    {
        public override string LabelString => null; // TODO: Button has no label?
        
        private readonly MethodInfo _methodInfo;

        public DrawableMethod(object instanceVal, MethodInfo method)
        {
            Host = instanceVal;
            _methodInfo = method;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _methodInfo?.Invoke(Host, Array.Empty<object>());
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            GUILayout.BeginArea(rect);
            DrawInner(label);
            GUILayout.EndArea();
        }

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_methodInfo == null)
                return base.GetDrawableAttributes<TAttribute>();
            return _methodInfo.GetCustomAttributes<TAttribute>();
        }
    }
}