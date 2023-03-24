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
        
        protected override T DrawValue(object instance, T memberVal)
        {
            EditorGUILayout.PrefixLabel(Label);
            return memberVal;
        }

        protected override T DrawValue(Rect rect, object instance, T memberVal)
        {
            EditorGUI.PrefixLabel(rect, Label);
            return memberVal;
        }
    }
}