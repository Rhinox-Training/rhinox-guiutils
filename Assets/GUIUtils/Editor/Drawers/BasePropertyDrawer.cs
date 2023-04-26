using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class ValuePropertyDrawer<T> : BasePropertyDrawer
    {
        private HostInfo _hostInfo;
        
        public T SmartValue
        {
            get => _hostInfo.GetSmartValue<T>();
            set => _hostInfo.TrySetValue(value);
        }
        
        protected override void Initialize()
        {
            _hostInfo = Property.GetHostInfo();
            
            base.Initialize();
        }
    }
    
    public abstract class BasePropertyDrawer : PropertyDrawer
    {
        protected SerializedProperty Property;
        
        private bool _initialized;
        protected Rect _rect;
        
        protected virtual void Initialize() {}
        
        protected abstract void DrawPropertyLayout(GUIContent label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Property = property;
            
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            GUILayout.BeginArea(_rect);
            var rect = EditorGUILayout.BeginVertical();

            DrawPropertyLayout(label);
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            if (position.IsValid())
            {
                _rect = position;
                _rect.height = rect.height;
            }
            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => _rect.height;
    }
}