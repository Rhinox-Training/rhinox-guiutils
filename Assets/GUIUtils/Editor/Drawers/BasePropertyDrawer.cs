using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
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

        public abstract Type FieldType { get; }
        public abstract GenericHostInfo HostInfo { get; }

        public event Action RepaintRequested;

        protected void Initialize()
        {
            if (_initialized) return;

            OnInitialize();
            _initialized = true;
        }

        protected virtual void OnInitialize()
        {
        }

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
            
            if (property != null)
                EditorGUI.BeginProperty(position, label, property);

            DrawProperty(position, label);
            
            if (property != null)
                EditorGUI.EndProperty();

            Apply();
            SaveData(property);
        }

        protected abstract void UpdateData(SerializedProperty property);


        protected virtual void SaveData(SerializedProperty property)
        {
        }

        protected virtual void Apply()
        {
            _property.serializedObject.ApplyModifiedProperties();
        }

        public void RequestRepaint()
        {
            RepaintRequested?.Invoke();
        }

        public virtual bool FindAttribute<T>(out T attribute) where T : Attribute
        {
            return FindAttribute<T>(out _, out attribute);
        }

        public virtual bool FindAttribute<T>(out GenericHostInfo hostInfo, out T attribute) where T : Attribute
        {
            attribute = null;
            hostInfo = null;
            if (HostInfo != null)
            {
                hostInfo = HostInfo;
                while (hostInfo != null)
                {
                    attribute = hostInfo.GetAttribute<T>();
                    if (attribute != null)
                        break;

                    var parent = hostInfo.Parent;
                    if (parent == null)
                        break;
                    hostInfo = parent;
                }
            }
            else if (_property != null)
            {
                Type searchType = FieldType;
                var property = _property;
                while (property != null)
                {
                    attribute = AttributeProcessorHelper.FindAttributeInclusive<T>(searchType);
                    if (attribute != null)
                        break;

                    var parent = property.FindParentProperty();
                    if (parent == null)
                        break;
                    searchType = parent.GetHostType();
                    property = parent;
                }
            }


            return attribute != null;
        }
    }

    public abstract class BasePropertyDrawer<T, TData> : BasePropertyDrawer, IHostInfoDrawer
    {
        // Property drawers are created per type - not per value, so we need to switch context when we get a different SerializedProperty
        private Dictionary<string, TData> _dataByPropertyPath;

        private string _activePath;
        private TData _activeData;

        private IOrderedDrawable _innerDrawable;

        public override Type FieldType => GetHostInfo(_activeData)?.GetReturnType();

        protected T SmartValue
        {
            get => HostInfo.GetSmartValue<T>();
            set => HostInfo.TrySetValue(value);
        }

        public override GenericHostInfo HostInfo => GetHostInfo(_activeData);

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _dataByPropertyPath = new Dictionary<string, TData>();
        }

        protected sealed override void DrawProperty(Rect position, GUIContent label)
        {
            if (!HostInfo.GetReturnType().InheritsFrom(typeof(T)))
            {
                CallInnerDrawer(position, label);
                return;
            }

            var oldActiveData = _activeData;
            DrawProperty(position, ref _activeData, label);
            if (!ReferenceEquals(oldActiveData, _activeData))
                UpdateActiveData(_activePath, _activeData);
        }

        protected abstract void DrawProperty(Rect position, ref TData data, GUIContent label);

        protected virtual float GetInnerDrawerHeight(GUIContent label)
        {
            if (_innerDrawable == null)
                return base.GetPropertyHeight(label);
            return _innerDrawable.ElementHeight;
        }

        private class TypeExclusionModifier<TFilter> : StandardDepthChecker
        {
            public TypeExclusionModifier()
            {
                if (typeof(TFilter) == typeof(object) || typeof(TFilter) == typeof(UnityEngine.Object))
                    throw new ArgumentException("TFilter cannot support object type");
            }
            
            public override DrawableCreationMode Find(GenericHostInfo hostInfo, int depth)
            {
                if (!CheckDepth(depth))
                    return DrawableCreationMode.None;
                var returnType = hostInfo.GetReturnType();
                if (returnType == typeof(TFilter))
                    return DrawableCreationMode.Composite;
                return DrawableCreationMode.Auto;
            }
        }

        protected virtual Rect CallInnerDrawer(Rect position, GUIContent label)
        {
            if (_innerDrawable == null)
                _innerDrawable = DrawableFactory.CreateDrawableFor(HostInfo, new TypeExclusionModifier<T>());
            float oldHeight = position.height;
            float innerDrawableHeight = _innerDrawable.ElementHeight;
            if (position.IsValid())
                position.height = innerDrawableHeight;
            _innerDrawable.Draw(position, label);
            if (position.IsValid())
            {
                position.height = oldHeight;    
                position.y += innerDrawableHeight;
            }
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
                var newData = CreateData(info);
                UpdateActiveData(path, newData);
            }
            else
                UpdateActiveData(path, _dataByPropertyPath[path]);

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

            TData dataForInfo;
            if (!_dataByPropertyPath.ContainsKey(key) || GetHostInfo(_dataByPropertyPath[key]) != info)
                dataForInfo = CreateData(info);
            else
                dataForInfo = _dataByPropertyPath[key];

            UpdateActiveData(key, dataForInfo);
        }

        private void UpdateActiveData(string path, TData data)
        {
            _dataByPropertyPath[path] = data;
            _activePath = path;
            var oldData = _activeData;
            _activeData = data;
            if (!ReferenceEquals(oldData, _activeData))
                OnUpdateActiveData();
        }

        protected virtual void OnUpdateActiveData()
        {
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