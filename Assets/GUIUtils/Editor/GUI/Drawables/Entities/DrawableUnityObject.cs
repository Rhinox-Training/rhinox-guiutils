using System;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityObject : BaseEntityDrawable<UnityEngine.Object>
    {
        public DrawableUnityObject(UnityEngine.Object instance, MemberInfo hostMemberInfo = null) 
            : base(instance, hostMemberInfo)
        {
        }

        protected override void DrawInner(GUIContent label)
        {
            EditorGUILayout.ObjectField(label, Entity, GetAppropriateType(), true);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
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