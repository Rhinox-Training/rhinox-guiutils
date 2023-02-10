using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class ObjectDrawableField : BaseSmartDrawableMember<object>
    {
        public ObjectDrawableField(MemberInfo info) : base(info) { }
        
        public override object DrawWithSmartValue(Object target)
        {
            var value = _info.GetValue(target);
            EditorGUILayout.LabelField(value.ToString());
            return value;
        }
    }
}