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

        public UnityObjectDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
            
        }
        
        protected override UnityEngine.Object DrawValue(GUIContent label, UnityEngine.Object memberVal, params GUILayoutOption[] options)
        {
            return EditorGUILayout.ObjectField(label, memberVal, HostInfo.GetReturnType(), AllowSceneObjects, options);
        }

        protected override UnityEngine.Object DrawValue(Rect rect, GUIContent label, UnityEngine.Object memberVal)
        {
            return EditorGUI.ObjectField(rect, label, memberVal, HostInfo.GetReturnType(), AllowSceneObjects);
        }
    }
}