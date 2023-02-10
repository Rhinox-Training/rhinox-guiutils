using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public static class DrawableMemberFactory
    {
        private const int MAX_DEPTH = 10;
        
        public static ICollection<IDrawableMember> CreateDrawableMembersFor(Type t)
        {
            return CreateDrawableMembersFor(t, 0);
        }

        private static ICollection<IDrawableMember> CreateDrawableMembersFor(Type t, int depth)
        {
            var publicAndSerializedMembers = SerializeHelper.GetPublicAndSerializedMembers(t);
            var drawableMembers = new List<IDrawableMember>();
            foreach (var memberInfo in publicAndSerializedMembers)
            {
                if (memberInfo == null)
                    continue;

                IDrawableMember resultingMember = null;
                if (!TryCreate(memberInfo, out var drawableMember) && depth < MAX_DEPTH)
                {
                    var subtype = memberInfo.GetReturnType();
                    var subdrawables = CreateDrawableMembersFor(subtype, depth + 1);
                    resultingMember = new CompositeDrawableMember(subdrawables, memberInfo);
                }
                else
                    resultingMember = drawableMember;

                if (resultingMember != null)
                    drawableMembers.Add(resultingMember);
            }

            return drawableMembers;
        }

        private static bool TryCreate(MemberInfo info, out IDrawableMember drawableMember)
        {
            var type = info.GetReturnType();

            if (type == typeof(string))
            {
                drawableMember = new StringDrawableField(info);
                return true;
            }

            if (type == typeof(int))
            {
                drawableMember = new IntDrawableField(info);
                return true;
            }

            if (type == typeof(float))
            {
                drawableMember = new FloatDrawableField(info);
                return true;
            }

            if (type == typeof(bool))
            {
                drawableMember = new BoolDrawableField(info);
                return true;
            }

            if (type.InheritsFrom<UnityEngine.Object>())
            {
                drawableMember = new UnityObjectDrawableField(info);
                return true;
            }

            drawableMember = null;
            return false;
        }
    }
}