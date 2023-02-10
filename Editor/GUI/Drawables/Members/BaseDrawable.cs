using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class BoolDrawableFieldAlt : BaseDrawable<bool>
    {
        public BoolDrawableFieldAlt(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override bool DrawWithSmartValue(object target)
        {
            return EditorGUILayout.Toggle(GetSmartValue());
        }
    }
    
    public abstract class BaseDrawable<T> : IOrderedDrawable
    {
        public int Order { get; set; }
        
        protected MemberInfo _info;
        private object _instance;

        public BaseDrawable(object instance, MemberInfo info)
        {
            _instance = instance;
            _info = info;
        }
        
        public T GetSmartValue() => (T) _info.GetValue(_instance);

        protected abstract T DrawWithSmartValue(object target);
        
        public void Draw()
        {
            
            var newVal = DrawWithSmartValue(_instance);
            _info.SetValue(_instance, newVal);
        }

        public void Draw(Rect rect)
        {
            Draw();
        }
    }
}