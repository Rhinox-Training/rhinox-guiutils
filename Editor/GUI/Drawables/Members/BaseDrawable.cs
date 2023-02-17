using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseDrawable<T> : IOrderedDrawable
    {
        public float Order { get; set; }

        public virtual float ElementHeight => EditorGUIUtility.singleLineHeight;
        
        public bool IsReadOnly { get; }
        public bool HideLabel { get; }

        public string Title { get; }

        public GUIContent Label => HideLabel ? GUIContent.none : GUIContentHelper.TempContent(_info.Name);
        
        protected MemberInfo _info;
        private object _instance;

        public BaseDrawable(object instance, MemberInfo info)
        {
            _instance = instance;
            _info = info;

            var orderAttr = _info.GetCustomAttribute<PropertyOrderAttribute>();
            if (orderAttr != null)
                Order = orderAttr.Order;
            
            var readonlyAttr = _info.GetCustomAttribute<ReadOnlyAttribute>();
            IsReadOnly = readonlyAttr != null || (_info is PropertyInfo propertyInfo && propertyInfo.GetSetMethod(true) == null);

            HideLabel = _info.GetCustomAttribute<HideLabelAttribute>() != null;

            var titleAttribute = _info.GetCustomAttribute<TitleAttribute>();
            Title = titleAttribute != null ? titleAttribute.Title : null;
        }

        protected T GetSmartValue() => (T) _info.GetValue(_instance);
        protected void SetSmartValue(T val) => _info.SetValue(_instance, val);

        public ICollection<TAttribute> GetMemberAttributes<TAttribute>() where TAttribute : Attribute
        {
            if (_info == null)
                return Array.Empty<TAttribute>();
            return _info.GetCustomAttributes<TAttribute>().ToArray();
        }

        public void Draw()
        {
            Update();
            var smartVal = GetSmartValue();
            TryDrawTitle();
            EditorGUI.BeginDisabledGroup(IsReadOnly);
            var newVal = DrawValue(_instance, smartVal);
            EditorGUI.EndDisabledGroup();
            if (!IsReadOnly)
                SetSmartValue(newVal);
        }

        public void Draw(Rect rect)
        {
            Update();
            var smartVal = GetSmartValue();
            TryDrawTitle();
            EditorGUI.BeginDisabledGroup(IsReadOnly);
            var newVal = DrawValue(rect, _instance, smartVal);
            EditorGUI.EndDisabledGroup();
            if (!IsReadOnly)
                SetSmartValue(newVal);
        }

        protected virtual void Update()
        {
            
        }

        protected abstract T DrawValue(object instance, T memberVal);
        protected abstract T DrawValue(Rect rect, object instance, T memberVal);

        private void TryDrawTitle()
        {
            if (!string.IsNullOrEmpty(Title))
            {
                EditorGUILayout.LabelField(Title, CustomGUIStyles.BoldTitle);
                CustomEditorGUI.HorizontalLine(new Color(200, 200, 200));
            }
        }
    }
}