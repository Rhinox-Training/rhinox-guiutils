using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UndrawableField : BaseMemberDrawable
    {
        public UndrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
            
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout.LabelField(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.PrefixLabel(rect, label);
        }
    }
}