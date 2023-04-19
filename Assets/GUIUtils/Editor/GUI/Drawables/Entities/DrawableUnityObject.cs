using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityObject : BaseEntityDrawable<UnityEngine.Object>
    {
        public DrawableUnityObject(UnityEngine.Object instance, MemberInfo hostMemberInfo = null) 
            : base(instance, hostMemberInfo)
        {
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            // TODO: should we ever set this value?
            EditorGUILayout.ObjectField(label, Entity, GetAppropriateType(), true, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            // TODO: should we ever set this value?
            EditorGUI.ObjectField(rect, label, Entity, GetAppropriateType(), true);
        }

        private Type GetAppropriateType()
        {
            if (Entity != null)
                return Entity.GetType();
            if (this._memberInfo != null)
                return _memberInfo.GetReturnType();
            return typeof(UnityEngine.Object);
        }
    }
}