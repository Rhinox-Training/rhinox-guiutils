using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class CompositeDrawableMember : IDrawableMember
    {
        private readonly ICollection<IDrawableMember> _drawableMemberChildren;
        private readonly MemberInfo _memberInfo;

        public CompositeDrawableMember(ICollection<IDrawableMember> subdrawables, MemberInfo memberInfo)
        {
            _drawableMemberChildren = subdrawables;
            _memberInfo = memberInfo;
        }

        public object Draw(object target)
        {
            if (_drawableMemberChildren == null)
                return target;

            var child = _memberInfo.GetValue(target);
            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                child = childDrawable.Draw(child);
            }

            _memberInfo.SetValue(target, child);
            return target;
        }
    }
}