using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawablePropertyView
    {
        private readonly object _instance;
        private readonly IReadOnlyCollection<MemberInfo> _serializedMembers;
        private readonly IDrawableMember[] _drawables;

        public DrawablePropertyView(object nonUnityObjInstance)
        {
            if (nonUnityObjInstance == null) throw new ArgumentNullException(nameof(nonUnityObjInstance));
            _instance = nonUnityObjInstance;
            _serializedMembers = SerializeHelper.GetPublicAndSerializedMembers(_instance.GetType());
            _drawables = _serializedMembers.Select(x => DrawableMemberFactory.Create(x)).ToArray();
        }
        
        public void Draw()
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw(_instance);
            }
        }
    }
}