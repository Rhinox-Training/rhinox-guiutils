using System;
using System.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class IntDrawableField : BaseSmartDrawableMember<int>
    {
        public IntDrawableField(MemberInfo info) : base(info) { }

        public override int DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.IntField(GetSmartValue(target));
        }
    }
}