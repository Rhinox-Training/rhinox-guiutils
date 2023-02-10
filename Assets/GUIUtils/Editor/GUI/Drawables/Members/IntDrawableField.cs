using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class IntDrawableField : BaseDrawable<int>
    {
        public IntDrawableField(object instance, MemberInfo info) : base(instance, info) { }

        protected override int DrawValue(object instance, int memberVal)
        {
            return EditorGUILayout.IntField(memberVal);
        }

        protected override int DrawValue(Rect rect, object instance, int memberVal)
        {
            return EditorGUI.IntField(rect, memberVal);
        }
    }
}