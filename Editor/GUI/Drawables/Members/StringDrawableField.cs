using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class StringDrawableField : BaseMemberDrawable<string>
    {
        public StringDrawableField(object instance, MemberInfo info) : base(instance, info) { }

        protected override string DrawValue(GUIContent label, string memberVal, params GUILayoutOption[] options)
        {
            return EditorGUILayout.TextField(label, memberVal, options);
        }

        protected override string DrawValue(Rect rect, GUIContent label, string memberVal)
        {
            return EditorGUI.TextField(rect, label, memberVal);
        }
    }
}