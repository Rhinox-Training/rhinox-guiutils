using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseEntityDrawable : BaseDrawable
    {
        protected readonly object _targetObj;

        protected override string LabelString => _targetObj != null ? _targetObj.GetType().Name : null;

        protected BaseEntityDrawable(object obj)
        {
            _targetObj = obj;
        }
        
        protected override void DrawInner(GUIContent label)
        {
            Draw(_targetObj);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            Draw(rect, _targetObj);
        }
        
        protected abstract void Draw(object target);
        protected abstract void Draw(Rect rect, object target);
    }
}