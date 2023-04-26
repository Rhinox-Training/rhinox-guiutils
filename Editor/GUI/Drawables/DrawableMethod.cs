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
        protected override string LabelString => null; // TODO: Button has no label?
        
        private readonly MethodInfo _methodInfo;
        
        private Rect _cachedRect;

        public DrawableMethod(GenericHostInfo info, MethodInfo method)
        {
            _hostInfo = new GenericHostInfo(info, method);
            _methodInfo = method;
        }
        
        public DrawableMethod(object instanceVal, MethodInfo method)
        {
            _hostInfo = new GenericHostInfo(instanceVal, method);
            _methodInfo = method;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _methodInfo?.Invoke(HostInfo.GetHost(), Array.Empty<object>());
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (rect.IsValid())
                _cachedRect = rect;
            
            GUILayout.BeginArea(_cachedRect, CustomGUIStyles.Clean);
            DrawInner(label);
            GUILayout.EndArea();
            
            if (!_cachedRect.IsValid() && Event.current.type == EventType.Layout)
                RequestRepaint();
        }

        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_methodInfo == null)
                return base.GetDrawableAttributes<TAttribute>();
            return _methodInfo.GetCustomAttributes<TAttribute>();
        }
    }
}