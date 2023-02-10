using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityObjectDrawableField : BaseDrawable<UnityEngine.Object>
    {
        public bool AllowSceneObjects = true;
        public UnityObjectDrawableField(object instance, MemberInfo info) : base(instance, info) { }
        
        protected override UnityEngine.Object DrawValue(object instance, UnityEngine.Object memberVal)
        {
            return EditorGUILayout.ObjectField(memberVal, _info.GetReturnType(), AllowSceneObjects);
        }

        protected override UnityEngine.Object DrawValue(Rect rect, object instance, UnityEngine.Object memberVal)
        {
            return EditorGUI.ObjectField(rect, memberVal, _info.GetReturnType(), AllowSceneObjects);
        }
    }
}