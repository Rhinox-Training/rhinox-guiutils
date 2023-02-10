using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectDrawableField : BaseDrawable<object>
    {
        public ObjectDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override object DrawValue(object instance, object memberVal)
        {
            EditorGUILayout.LabelField(memberVal.ToString());
            return memberVal;
        }

        protected override object DrawValue(Rect rect, object instance, object memberVal)
        {
            EditorGUI.LabelField(rect, memberVal.ToString());
            return memberVal;
        }
    }
}