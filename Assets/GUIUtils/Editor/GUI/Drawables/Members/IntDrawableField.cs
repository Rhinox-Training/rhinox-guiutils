using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class IntDrawableField : BaseMemberDrawable<int>
    {
        public IntDrawableField(object instance, MemberInfo info) : base(instance, info) { }

        protected override int DrawValue(object instance, int memberVal)
        {
            return EditorGUILayout.IntField(Label, memberVal);
        }

        protected override int DrawValue(Rect rect, object instance, int memberVal)
        {
            return EditorGUI.IntField(rect, Label, memberVal);
        }
    }
}