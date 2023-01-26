using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableObject : ISimpleDrawable
    {
        public int Order { get; set; }
        
        private object _instance;
        private readonly Type _type;
        private readonly IReadOnlyCollection<MemberInfo> _members;
        private readonly IDrawableMember[] _drawableMemberProperties;

        public DrawableObject(object obj, int order = 0)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            _instance = obj;
            _type = obj.GetType();
            _members = SerializeHelper.GetPublicAndSerializedMembers(_type);
            _drawableMemberProperties = _members.Select(x => DrawableMemberFactory.Create(x)).ToArray();
        }

        public void Draw()
        {
            foreach (var prop in _drawableMemberProperties)
            {
                if (prop == null)
                    continue;
                _instance = prop.Draw(_instance);
            }
        }

        public void Draw(Rect rect)
        {
            GUILayout.BeginArea(rect);
            Draw();
            GUILayout.EndArea();
        }
    }
}