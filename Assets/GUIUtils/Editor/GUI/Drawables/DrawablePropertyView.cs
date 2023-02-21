using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawablePropertyView
    {
        private readonly object _instance;
        private readonly ICollection<IOrderedDrawable> _drawables;

        public float Height
        {
            get
            {
                float height = 0.0f;
                foreach (var drawable in _drawables)
                    height += drawable.ElementHeight;
                return height;
            }
        }

        public DrawablePropertyView(object unityObjInstance, bool forceDrawAsUnityObject = false)
        {
            if (unityObjInstance == null) throw new ArgumentNullException(nameof(unityObjInstance));
            _instance = unityObjInstance;
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject(unityObjInstance)};
            else
                _drawables = DrawableFactory.ParseNonUnityObject(unityObjInstance);
        }
        
        public DrawablePropertyView(SerializedObject serializedObject, bool forceDrawAsUnityObject = false)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            _instance = serializedObject;
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject(serializedObject.targetObject)};
            else
                _drawables = DrawableFactory.ParseSerializedObject(serializedObject);
        }
        
        public DrawablePropertyView(SerializedProperty property, bool forceDrawAsUnityObject = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _instance = property;
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject(property.GetValue())};
            else
                _drawables = DrawableFactory.ParseSerializedProperty(property);
        }
        
        public void DrawLayout()
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw();
            }
        }
        
        public void Draw(Rect rect)
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                rect.height = drawable.ElementHeight;
                drawable.Draw(rect);
                rect.y += rect.height;
            }
        }
    }
}