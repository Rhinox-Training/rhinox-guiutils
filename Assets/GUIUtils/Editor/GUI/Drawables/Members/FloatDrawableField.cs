using System;
using System.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class FloatDrawableField : BaseSmartDrawableMember<float>
    {
        public FloatDrawableField(MemberInfo info) : base(info) { }
        
        public override float DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.FloatField(GetSmartValue(target));
        }
    }
}