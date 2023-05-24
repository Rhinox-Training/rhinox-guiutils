using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityObjectDrawableField : BaseMemberValueDrawable<UnityEngine.Object>
    {
        public bool AllowSceneObjects = true;

        public UnityObjectDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
            AllowSceneObjects = hostInfo.GetReturnType().GetCustomAttribute<AssetsOnlyAttribute>() == null;
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