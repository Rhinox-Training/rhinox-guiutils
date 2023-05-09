using System;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IHostInfoDrawer
    {
        GenericHostInfo HostInfo { get; set; }
        event Action RepaintRequested;
    }
    
    public abstract class BasePropertyDrawer : PropertyDrawer
    {
        protected SerializedProperty _property;
        private bool _initialized;
        
        public event Action RepaintRequested;
        
        protected virtual void Initialize() {}
        
        protected virtual void DrawProperty(Rect position, GUIContent label)
        {
            
        }

        protected virtual float GetPropertyHeight(GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _property = property;
            
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }
            
            return GetPropertyHeight(label);
        }
        
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _property = property;

            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            DrawProperty(position, label);
        }

        protected virtual void Apply()
        {
            _property.serializedObject.ApplyModifiedProperties();
        }

        protected void RequestRepaint()
        {
            RepaintRequested?.Invoke();
        }
    }
    
    public abstract class BasePropertyDrawer<T> : BasePropertyDrawer, IHostInfoDrawer
    {
        public GenericHostInfo HostInfo { get; set; }
        protected Type FieldType;
        
        public T SmartValue
        {
            get => HostInfo.GetSmartValue<T>();
            set => HostInfo.TrySetValue(value);
        }
        
        protected override void Initialize()
        {
            base.Initialize();

            if (HostInfo == null)
                HostInfo = _property.GetHostInfo();
            
            FieldType = HostInfo.GetReturnType(false);
        }

        protected override void Apply()
        {
            HostInfo.Apply();
        }
    }
}