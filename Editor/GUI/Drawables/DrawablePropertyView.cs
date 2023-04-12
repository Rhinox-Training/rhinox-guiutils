using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawablePropertyView : IEditor
    {
        private readonly object _instance;
        private readonly GenericMemberEntry _entry;
        private readonly SerializedObject _serializedObject;
        private readonly IOrderedDrawable _rootDrawable;

        public bool ShouldRepaint { get; private set; }

        public float Height => _rootDrawable != null ? _rootDrawable.ElementHeight : 0.0f;

        public DrawablePropertyView(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _instance = instance;
            _entry = null;
            _serializedObject = null;
            
            _rootDrawable = DrawableFactory.CreateDrawableFor(_instance, _instance.GetType());
        }
        
        public DrawablePropertyView(GenericMemberEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            _entry = entry;
            _instance = _entry.Instance;
            _serializedObject = null;

            _rootDrawable = DrawableFactory.CreateDrawableFor(entry);
        }
        
        public DrawablePropertyView(SerializedObject serializedObject)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            _instance = serializedObject;
            _serializedObject = serializedObject;
            _entry = null;
            
            _rootDrawable = DrawableFactory.CreateDrawableFor(_serializedObject, _serializedObject.targetObject.GetType());
        }
        
        public DrawablePropertyView(SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _instance = property;
            _serializedObject = property.serializedObject;
            _entry = null;
            
            _rootDrawable = DrawableFactory.CreateDrawableFor(property, true);
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
            rect.height = Height;
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
            if (_rootDrawable != null && _rootDrawable.ShouldRepaint)
                ShouldRepaint = true;
            
            if (_serializedObject != null)
                _serializedObject.ApplyModifiedProperties();
        }
        
        internal void MarkAsRepainted()
        {
            ShouldRepaint = false;
        }

        public bool HasPreviewGUI() => false;

        public void DrawPreview(Rect rect) { }

        public bool CanDraw() => true;

        public void Draw()
        {
            DrawLayout();
        }

        public void Destroy()
        {
            // Nothing to do
        }
    }
}