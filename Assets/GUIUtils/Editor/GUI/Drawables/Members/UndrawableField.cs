using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UndrawableField<T> : BaseMemberDrawable<T>
    {
        public UndrawableField(object instance, MemberInfo info) : base(instance, info)
        {
            
        }
        
        protected override T DrawValue(GUIContent label, T memberVal, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(label, options);
            return memberVal;
        }

        protected override T DrawValue(Rect rect, GUIContent label, T memberVal)
        {
            EditorGUI.PrefixLabel(rect, label);
            return memberVal;
        }
    }
}