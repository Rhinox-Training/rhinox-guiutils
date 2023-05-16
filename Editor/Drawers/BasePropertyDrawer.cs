using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IHostInfoDrawer
    {
        void SetupForHostInfo(GenericHostInfo info, string key);
    }
    
    public abstract class BasePropertyDrawer : PropertyDrawer, IRepaintEvent, IRepaintable
    {
        private SerializedProperty _property;
        private bool _initialized;
        
        public event Action RepaintRequested;

        protected void Initialize()
        {
            if (_initialized) return;
            
            OnInitialize();
            _initialized = true;
        }
        
        protected virtual void OnInitialize() {}

        protected abstract void DrawProperty(Rect position, GUIContent label);

        protected virtual float GetPropertyHeight(GUIContent label)
            => EditorGUIUtility.singleLineHeight;

        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize();

            _property = property;
            UpdateData(property);
            
            return GetPropertyHeight(label);
        }
        
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        { 
            Initialize();
            
            _property = property;
            UpdateData(property);

            DrawProperty(position, label);

            Apply();
            SaveData(property);
        }

        protected abstract void UpdateData(SerializedProperty property);


        protected virtual void SaveData(SerializedProperty property)
        { }
        
        protected virtual void Apply()
        {
            _property.serializedObject.ApplyModifiedProperties();
        }

        public void RequestRepaint()
        {
            RepaintRequested?.Invoke();
        }
    }
    
    public abstract class BasePropertyDrawer<T, TData> : BasePropertyDrawer, IHostInfoDrawer
    {
        
        // Property drawers are created per type - not per value, so we need to switch context when we get a different SerializedProperty
        private Dictionary<string, TData> _dataByPropertyPath;

        private TData _activeData;
        private IOrderedDrawable _innerDrawable;

        protected Type FieldType => GetHostInfo(_activeData).GetReturnType();
        
        protected T SmartValue
        {
            get => HostInfo.GetSmartValue<T>();
            set => HostInfo.TrySetValue(value);
        }

        private GenericHostInfo HostInfo => GetHostInfo(_activeData);

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _dataByPropertyPath = new Dictionary<string, TData>();    
        }

        protected sealed override void DrawProperty(Rect position, GUIContent label)
        {
            DrawProperty(position, ref _activeData, label);
        }

        protected abstract void DrawProperty(Rect position, ref TData data, GUIContent label);

        protected virtual float GetInnerDrawerHeight(GUIContent label)
        {
            if (_innerDrawable == null)
                return base.GetPropertyHeight(label);
            return _innerDrawable.ElementHeight;
        }
        
        protected virtual Rect CallInnerDrawer(Rect position, GUIContent label)
        {
            if (_innerDrawable == null)
                _innerDrawable = DrawableFactory.CreateDrawableFor(HostInfo, false);
            float oldHeight = position.height;
            float innerDrawableHeight = _innerDrawable.ElementHeight;
            position.height = innerDrawableHeight;
            _innerDrawable.Draw(position, label);
            position.height = oldHeight;
            position.y += innerDrawableHeight;
            return position;
        }
        
        protected sealed override float GetPropertyHeight(GUIContent label)
            => GetPropertyHeight(label, in _activeData);

        protected virtual float GetPropertyHeight(GUIContent label, in TData data)
            => EditorGUIUtility.singleLineHeight;
        
        protected override void UpdateData(SerializedProperty property)
        {
            if (property == null) return;

            var path = property.propertyPath;

            if (!_dataByPropertyPath.ContainsKey(path))
            {

                var info = property.GetHostInfo();
                _activeData = CreateData(info);

                _dataByPropertyPath[path] = _activeData;
            }
            else
                _activeData = _dataByPropertyPath[path];
            
            OnUpdateData();
        }

        protected sealed override void SaveData(SerializedProperty property)
        {
            if (property == null)
                return;
            OnSaveData(_activeData, property.propertyPath);
        }

        protected virtual void OnSaveData(TData data, string key)
        {
            _dataByPropertyPath[key] = data;
        }

        protected virtual void OnUpdateData()
        {
        }

        public void SetupForHostInfo(GenericHostInfo info, string key)
        {
            Initialize();

            if (!_dataByPropertyPath.ContainsKey(key))
                _dataByPropertyPath[key] = CreateData(info);
            
            _activeData = _dataByPropertyPath[key];
        }

        protected abstract TData CreateData(GenericHostInfo info);

        protected abstract GenericHostInfo GetHostInfo(TData data);
        
        protected override void Apply()
        {
            HostInfo?.Apply();
        }
    }
    
    public abstract class BasePropertyDrawer<T> : BasePropertyDrawer<T, GenericHostInfo>
    {
        protected override GenericHostInfo GetHostInfo(GenericHostInfo data) => data;
        
        protected override GenericHostInfo CreateData(GenericHostInfo info) => info;
    }
}