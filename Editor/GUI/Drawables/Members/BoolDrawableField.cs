using System;
using System.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class BoolDrawableField : BaseSmartDrawableMember<bool>
    {
        public BoolDrawableField(MemberInfo info) : base(info) { }
        
        public override bool DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.Toggle(GetSmartValue(target));
        }
    }
}