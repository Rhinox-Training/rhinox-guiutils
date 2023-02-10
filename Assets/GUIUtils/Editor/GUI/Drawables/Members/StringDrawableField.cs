using System;
using System.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class StringDrawableField : BaseSmartDrawableMember<string>
    {
        public StringDrawableField(MemberInfo info) : base(info) { }

        public override string DrawWithSmartValue(Object target)
        {
            return EditorGUILayout.TextField(GetSmartValue(target));
        }
    }
}