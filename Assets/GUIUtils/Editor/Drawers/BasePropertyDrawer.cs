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
        private Dictionary<GenericHostInfo, IOrderedDrawable> _innerDrawableByHostInfo;

        private string _activePath;
        private TData _activeData;

        private CustomPropertyDrawer _customPropertyDrawerAttr;

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
            _innerDrawableByHostInfo = new Dictionary<GenericHostInfo, IOrderedDrawable>();
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
            var innerDrawable = GetInnerDrawable(out _);
            if (innerDrawable == null)
                return base.GetPropertyHeight(label);
            return innerDrawable.ElementHeight;
        }

        private IOrderedDrawable GetInnerDrawable(out GenericHostInfo info)
        {
            info = GetHostInfo(_activeData);
            var innerDrawable = _innerDrawableByHostInfo.GetOrDefault(info);
            return innerDrawable;
        }

        private class TypeExclusionModifier : StandardDepthChecker
        {
            public Type FilterType { get; }

            public bool InheritsFrom { get; }
            
            public TypeExclusionModifier(Type filterType, bool inheritsFrom = false)
            {
                if (filterType == typeof(object) || filterType == typeof(UnityEngine.Object))
                    throw new ArgumentException("TFilter cannot support object type");
                FilterType = filterType;
                InheritsFrom = inheritsFrom;
            }

            public override DrawableCreationMode Find(GenericHostInfo hostInfo, int depth)
            {
                if (!CheckDepth(depth))
                    return DrawableCreationMode.None;
                var returnType = hostInfo.GetReturnType();
                if (CheckType(returnType))
                    return DrawableCreationMode.Composite;
                return DrawableCreationMode.Auto;
            }

            public override bool ShouldWrap(GenericHostInfo hostInfo, int depth)
            {
                var returnType = hostInfo.GetReturnType();
                if (CheckType(returnType))
                    return false;
                return base.ShouldWrap(hostInfo, depth);
            }

            private bool CheckType(Type returnType)
            {
                if (InheritsFrom)
                    return returnType.InheritsFrom(FilterType);
                return returnType == FilterType;
            }
        }

        private Type GetTargetTypeOfPropertyDrawer()
        {
            if (_customPropertyDrawerAttr == null)
                _customPropertyDrawerAttr = this.GetType().GetCustomAttribute<CustomPropertyDrawer>();
            return _customPropertyDrawerAttr.GetPropertyType();
        }

        private bool IsPropertyDrawerForChildTypes()
        {
            if (_customPropertyDrawerAttr == null)
                _customPropertyDrawerAttr = this.GetType().GetCustomAttribute<CustomPropertyDrawer>();
            return _customPropertyDrawerAttr.IsUsedForChildren();
        }

        protected virtual Rect CallInnerDrawer(Rect position, GUIContent label)
        {
            var innerDrawable = GetInnerDrawable(out GenericHostInfo info);
            if (innerDrawable == null)
            {
                var propertyType = GetTargetTypeOfPropertyDrawer();
                var modifier = propertyType.InheritsFrom<Attribute>()
                    ? null
                    : new TypeExclusionModifier(propertyType, IsPropertyDrawerForChildTypes());
                innerDrawable = DrawableFactory.CreateDrawableFor(HostInfo, modifier);
                _innerDrawableByHostInfo[info] = innerDrawable;
            }
            
            float oldHeight = position.height;
            float innerDrawableHeight = innerDrawable.ElementHeight;
            if (position.IsValid())
                position.height = innerDrawableHeight;
            innerDrawable.Draw(position, label);
            if (position.IsValid())
            {
                position.height = oldHeight;    
                position.yMin += innerDrawableHeight;
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
        
        private class ChildDrawer : IEditorDrawable
        {
            private IOrderedDrawable _childDrawable;

            public ChildDrawer(GenericHostInfo hostInfo)
            {
                _childDrawable = DrawableFactory.CreateDrawableFor(hostInfo);
            }

            public float ElementHeight => _childDrawable.ElementHeight;

            public void Draw(GUIContent label, params GUILayoutOption[] options)
            {
                _childDrawable.Draw(label, options);
            }

            public void Draw(Rect position, GUIContent label)
            {
                float oldHeight = position.height;
                float innerDrawableHeight = _childDrawable.ElementHeight;
                if (position.IsValid())
                    position.height = innerDrawableHeight;
                _childDrawable.Draw(position, label);
                if (position.IsValid())
                {
                    position.height = oldHeight;    
                    position.y += innerDrawableHeight;
                }
            }
        }

        protected IEditorDrawable GetChildDrawer(string memberDataName, int index = -1)
        {
            return GetChildDrawer(memberDataName, out _, index);
        }

        protected IEditorDrawable GetChildDrawer(string memberDataName, out GenericHostInfo childHostInfo, int index = -1)
        {
            HostInfo.TryGetChild(memberDataName, out childHostInfo, index);
            return GetChildDrawer(childHostInfo);
        }

        protected IEditorDrawable GetChildDrawer(GenericHostInfo childHostInfo)
        {
            var childDrawer = new ChildDrawer(childHostInfo);
            return childDrawer;
        }
    }

    public abstract class BasePropertyDrawer<T> : BasePropertyDrawer<T, GenericHostInfo>
    {
        protected override GenericHostInfo GetHostInfo(GenericHostInfo data) => data;

        protected override GenericHostInfo CreateData(GenericHostInfo info) => info;
    }
}