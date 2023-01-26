using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public interface IDrawableMember
    {
        object Draw(Object target);
    }

    public abstract class SmartDrawableMember<T> : IDrawableMember
    {
        protected MemberInfo _info;
        
        public object Draw(Object target) => DrawWithSmartValue(target);

        public T GetSmartValue(Object target) => (T) _info.GetValue(target);

        public abstract T DrawWithSmartValue(Object target);
        
        public SmartDrawableMember(MemberInfo info)
        {
            _info = info;
        }
    }

    public class StringDrawableField : SmartDrawableMember<string>
    {
        public StringDrawableField(MemberInfo info) : base(info) { }

        public override string DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.TextField(GetSmartValue(target));
        }
    }
    
    public class IntDrawableField : SmartDrawableMember<int>
    {
        public IntDrawableField(MemberInfo info) : base(info) { }

        public override int DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.IntField(GetSmartValue(target));
        }
    }
    
    public class FloatDrawableField : SmartDrawableMember<float>
    {
        public FloatDrawableField(MemberInfo info) : base(info) { }
        
        public override float DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.FloatField(GetSmartValue(target));
        }
    }
    
    public class BoolDrawableField : SmartDrawableMember<bool>
    {
        public BoolDrawableField(MemberInfo info) : base(info) { }
        
        public override bool DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.Toggle(GetSmartValue(target));
        }
    }
    
    public class ObjectDrawableField : SmartDrawableMember<UnityEngine.Object>
    {
        public bool AllowSceneObjects = true;
        public ObjectDrawableField(MemberInfo info) : base(info) { }
        
        public override UnityEngine.Object DrawWithSmartValue(Object target)
        {
            var val = GetSmartValue(target);
            return EditorGUILayout.ObjectField(val, _info.GetReturnType(), AllowSceneObjects);
        }
    }
    
    public class LabelDrawableField : SmartDrawableMember<object>
    {
        public LabelDrawableField(MemberInfo info) : base(info) { }
        
        public override object DrawWithSmartValue(Object target)
        {
            var value = _info.GetValue(target);
            EditorGUILayout.LabelField(value.ToString());
            return value;
        }
    }
    
    public static class DrawableMemberFactory
    {
        public static IDrawableMember Create(MemberInfo info)
        {
            var type = info.GetReturnType();
            
            if (type == typeof(string))
                return new StringDrawableField(info);
            
            if (type == typeof(int))
                return new IntDrawableField(info);

            if (type == typeof(float))
                return new FloatDrawableField(info);

            if (type == typeof(bool))
                return new BoolDrawableField(info);

            if (type.InheritsFrom<UnityEngine.Object>())
                return new ObjectDrawableField(info);
            
            return new CompositeDrawableMember(type);
        }
    }
}