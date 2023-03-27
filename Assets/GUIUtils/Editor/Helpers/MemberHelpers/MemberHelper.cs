using System;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public static class MemberHelper
    {
        public static IPropertyMemberHelper<T> Create<T>(object target, string input)
        {
            if (target is SerializedProperty property)
                return CreateSerialized<T>(property, input);
            return CreateGeneric<T>(target, input);
        }
        
        public static IPropertyMemberHelper<T> CreateSerialized<T>(SerializedProperty target, string input)
        {
            return new SerializedPropertyMemberHelper<T>(target, input);
        }
        
        public static IPropertyMemberHelper<T> CreateGeneric<T>(object target, string input)
        {
            return new GenericPropertyMemberHelper<T>(target, input);
        }
        
        public static MethodMemberHelper CreateMethod(object target, string input)
        {
            return new MethodMemberHelper(target, input);
        }

    }
}