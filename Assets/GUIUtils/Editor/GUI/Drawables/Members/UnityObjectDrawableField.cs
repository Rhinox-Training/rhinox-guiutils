using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityObjectDrawableField : BaseMemberDrawable<UnityEngine.Object>
    {
        public bool AllowSceneObjects = true;

        public UnityObjectDrawableField(object instance, MemberInfo info) : base(instance, info)
        {
            
        }
        
        protected override UnityEngine.Object DrawValue(GUIContent label, UnityEngine.Object memberVal, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ObjectField(label, memberVal, _info.GetReturnType(), AllowSceneObjects, options);
        }

        protected override UnityEngine.Object DrawValue(Rect rect, GUIContent label, UnityEngine.Object memberVal)
        {
            return EditorGUI.ObjectField(rect, label, memberVal, _info.GetReturnType(), AllowSceneObjects);
        }
    }
}