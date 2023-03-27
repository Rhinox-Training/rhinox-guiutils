using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectDrawableField : BaseMemberDrawable<object>
    {
        public ObjectDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override object DrawValue(GUIContent label, object memberVal)
        {
            EditorGUILayout.LabelField(label);
            return memberVal;
        }

        protected override object DrawValue(Rect rect, GUIContent label, object memberVal)
        {
            EditorGUI.LabelField(rect, label);
            return memberVal;
        }
    }
}