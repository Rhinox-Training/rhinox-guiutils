using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityObjectDrawableField : BaseSmartDrawableMember<UnityEngine.Object>
    {
        public bool AllowSceneObjects = true;
        public UnityObjectDrawableField(MemberInfo info) : base(info) { }
        
        public override UnityEngine.Object DrawWithSmartValue(Object target)
        {
            var val = GetSmartValue(target);
            return EditorGUILayout.ObjectField(val, _info.GetReturnType(), AllowSceneObjects);
        }
    }
}