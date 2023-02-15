using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class StringDrawableField : BaseDrawable<string>
    {
        public StringDrawableField(object instance, MemberInfo info) : base(instance, info) { }

        protected override string DrawValue(object instance, string memberVal)
        {
            return EditorGUILayout.TextField(Label, memberVal);
        }

        protected override string DrawValue(Rect rect, object instance, string memberVal)
        {
            return EditorGUI.TextField(rect, Label, memberVal);
        }
    }
}