using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class FloatDrawableField : BaseMemberDrawable<float>
    {
        public FloatDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override float DrawValue(GUIContent label, float memberVal)
        {
            return EditorGUILayout.FloatField(label, memberVal);
        }

        protected override float DrawValue(Rect rect, GUIContent label, float memberVal)
        {
            return EditorGUI.FloatField(rect, label, memberVal);
        }
    }
}