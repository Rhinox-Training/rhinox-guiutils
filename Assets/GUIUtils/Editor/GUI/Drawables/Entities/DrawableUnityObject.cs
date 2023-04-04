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
        private readonly GenericMemberEntry _genericMemberEntry;

        public DrawableUnityObject(UnityEngine.Object instance, MemberInfo hostMemberInfo = null) 
            : base(instance, hostMemberInfo)
        {
        }

        public DrawableUnityObject(GenericMemberEntry entry) : base((UnityEngine.Object) entry.GetValue(), entry.Info)
        {
            _genericMemberEntry = entry;
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
            if (_genericMemberEntry != null)
                return _genericMemberEntry.GetReturnType();
            if (this._memberInfo != null)
                return _memberInfo.GetReturnType();
            return typeof(UnityEngine.Object);
        }
    }
}