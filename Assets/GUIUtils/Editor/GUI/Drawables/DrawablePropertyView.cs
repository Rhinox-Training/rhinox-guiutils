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
        private readonly GenericMemberEntry _entry;
        private readonly SerializedObject _serializedObject;
        private readonly IOrderedDrawable _rootDrawable;

        public float Height => _rootDrawable != null ? _rootDrawable.ElementHeight : 0.0f;

        public DrawablePropertyView(object instance, bool forceDrawAsUnityObject = false)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _instance = instance;
            _entry = null;
            _serializedObject = null;
            
            if (forceDrawAsUnityObject)
                _rootDrawable = new DrawableUnityObject((UnityEngine.Object) instance);
            else
                _rootDrawable = ParseNonUnityObject(instance);
        }
        
        public DrawablePropertyView(GenericMemberEntry entry, bool forceDrawAsUnityObject = false)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            _entry = entry;
            _instance = _entry.Instance;
            _serializedObject = null;

            if (forceDrawAsUnityObject)
                _rootDrawable = new DrawableUnityObject((UnityEngine.Object) entry.GetValue(), entry.Info);
            else
                _rootDrawable = DrawableFactory.CreateDrawableFor(entry);
        }
        
        public DrawablePropertyView(SerializedObject serializedObject, bool forceDrawAsUnityObject = false)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            _instance = serializedObject;
            _serializedObject = serializedObject;
            _entry = null;
            
            if (forceDrawAsUnityObject)
                _rootDrawable = new DrawableUnityObject(serializedObject.targetObject);
            else
                _rootDrawable = ParseSerializedObject(serializedObject);
        }
        
        public DrawablePropertyView(SerializedProperty property, bool forceDrawAsUnityObject = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _instance = property;
            _serializedObject = property.serializedObject;
            _entry = null;
            
            if (forceDrawAsUnityObject)
                _rootDrawable = new DrawableUnityObject((UnityEngine.Object) property.GetValue());
            else
                _rootDrawable = ParseSerializedProperty(property);
        }
        
        protected static IOrderedDrawable ParseNonUnityObject(object obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();

            var drawable = DrawableFactory.CreateDrawableFor(obj, type);

            if (drawable == null && obj is UnityEngine.Object unityObj)
                drawable = new DrawableUnityObject(unityObj, null);

            return drawable;
        }

        protected static IOrderedDrawable ParseSerializedProperty(SerializedProperty property)
        {
            if (property == null)
                return null;

            var hostInfo = property.GetHostInfo();
            var type = hostInfo.GetReturnType();

            if (AttributeParser.ParseDrawAsUnity(hostInfo.FieldInfo))
                return new DrawableUnityObject((UnityEngine.Object)property.GetValue(), property.FindFieldInfo());

            var drawable = DrawableFactory.CreateDrawableFor(property, type);
            return drawable;
        }

        protected static IOrderedDrawable ParseSerializedObject(SerializedObject obj)
        {
            if (obj == null || obj.targetObject == null)
                return null;

            var type = obj.targetObject.GetType();

            var drawable = DrawableFactory.CreateDrawableFor(obj, type);
            return drawable;
        }
        
        public void DrawLayout()
        {
            if (_rootDrawable == null)
                return;

            OnPreDraw();
            
            _rootDrawable.Draw(_rootDrawable.Label);

            OnPostDraw();
        }

        public void Draw(Rect rect)
        {
            if (_rootDrawable == null)
                return;
            
            OnPreDraw();
            
            
            // TODO: should we force height?
            //rect.height = Height;
            _rootDrawable.Draw(rect, _rootDrawable.Label);
            
            OnPostDraw();
        }

        private void OnPreDraw()
        {
            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }
        }

        private void OnPostDraw()
        {
            if (_serializedObject != null)
                _serializedObject.ApplyModifiedProperties();
        }
    }
}