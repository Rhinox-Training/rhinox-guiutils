using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class InfoBoxWrapper : WrapperDrawable
    {
        private IPropertyMemberHelper<bool> _member;
        private string _message;
        private MessageType _type;
        
        public InfoBoxWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            if (_member == null || _member.GetValue())
                EditorGUILayout.HelpBox(_message, _type);
            base.OnPreDraw();
        }

        [WrapDrawer(typeof(InfoBoxAttribute), -5000)]
        public static WrapperDrawable Create(InfoBoxAttribute attr, IOrderedDrawable drawable)
        {
            var member = MemberHelper.Create<bool>(drawable.Host, attr.VisibleIf);
            return new InfoBoxWrapper(drawable)
            {
                _message = attr.Message,
                _type = ToMessageType(attr.InfoMessageType),
                _member = member
            };
        }

        private static MessageType ToMessageType(InfoMessageType type)
        {
            switch (type)
            {
                case InfoMessageType.None:
                    return MessageType.None;
                case InfoMessageType.Info:
                    return MessageType.Info;
                case InfoMessageType.Warning:
                    return MessageType.Warning;
                case InfoMessageType.Error:
                    return MessageType.Error;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}