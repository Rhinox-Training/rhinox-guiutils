using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class FloatDrawableField : BaseDrawable<float>
    {
        public FloatDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override float DrawValue(object instance, float memberVal)
        {
            return EditorGUILayout.FloatField(memberVal);
        }

        protected override float DrawValue(Rect rect, object instance, float memberVal)
        {
            return EditorGUI.FloatField(rect, memberVal);
        }
    }
}