using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class IntDrawableField : BaseMemberDrawable<int>
    {
        public IntDrawableField(object instance, MemberInfo info) : base(instance, info) { }

        protected override int DrawValue(GUIContent label, int memberVal)
        {
            return EditorGUILayout.IntField(label, memberVal);
        }

        protected override int DrawValue(Rect rect, GUIContent label, int memberVal)
        {
            return EditorGUI.IntField(rect, label, memberVal);
        }
    }
}