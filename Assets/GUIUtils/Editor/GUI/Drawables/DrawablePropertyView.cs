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
        private readonly GenericHostInfo _hostInfo;
        private readonly SerializedObject _serializedObject;
        private readonly IOrderedDrawable _rootDrawable;
        
        public float Height => _rootDrawable.ElementHeight;

        public event Action RepaintRequested;

        public DrawablePropertyView(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _instance = instance;
            _hostInfo = null;
            _serializedObject = null;
            
            _rootDrawable = DrawableFactory.CreateDrawableFor(_instance);
            _rootDrawable.RepaintRequested += RequestRepaint;
        }
        
        public DrawablePropertyView(GenericHostInfo hostInfo)
        {
            if (hostInfo == null) throw new ArgumentNullException(nameof(hostInfo));
            _instance = hostInfo.GetHost();
            _hostInfo = hostInfo;
            _serializedObject = null;

            _rootDrawable = DrawableFactory.CreateDrawableFor(hostInfo);
            _rootDrawable.RepaintRequested += RequestRepaint;

        }
        
        public DrawablePropertyView(SerializedObject serializedObject)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            _instance = serializedObject;
            _hostInfo = null;
            _serializedObject = serializedObject;
            
            _rootDrawable = DrawableFactory.CreateDrawableFor(_serializedObject);
            _rootDrawable.RepaintRequested += RequestRepaint;
        }
        
        public DrawablePropertyView(SerializedProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _instance = property;
            _hostInfo = property.GetHostInfo();
            _serializedObject = property.serializedObject;
            
            _rootDrawable = DrawableFactory.CreateDrawableFor(property);
            _rootDrawable.RepaintRequested += RequestRepaint;
        }
        
        public void DrawLayout()
        {
            if (_rootDrawable == null)
                return;

            OnPreDraw();
            _rootDrawable.Draw(GUIContent.none);
            OnPostDraw();
        }

        public void Draw(Rect rect)
        {
            if (_rootDrawable == null)
                return;
            
            OnPreDraw();
            
            // TODO: should we force height?
            if (rect.IsValid())
                rect.height = Height;
            
            _rootDrawable.Draw(rect, GUIContent.none);
            
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
        
        internal void RequestRepaint()
        {
            RepaintRequested?.Invoke();
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