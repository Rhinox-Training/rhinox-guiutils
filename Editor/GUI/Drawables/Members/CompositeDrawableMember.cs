using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class CompositeDrawableMember : IDrawableMember
    {
        private readonly Type _type;
        private readonly IReadOnlyCollection<MemberInfo> _members;
        private readonly IDrawableMember[] _drawableMemberProperties;

        public CompositeDrawableMember(Type t)
        {
            _type = t;
            _members = SerializeHelper.GetPublicAndSerializedMembers(_type);
            _drawableMemberProperties = _members.Select(x => DrawableMemberFactory.Create(x)).ToArray();
        }
        
        public object Draw(object target)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(_type.Name, EditorStyles.boldLabel);
            foreach (var prop in _drawableMemberProperties)
            {
                if (prop == null)
                    continue;
                target = prop.Draw(target);
            }
            EditorGUILayout.EndVertical();
            return target;
        }
    }
}