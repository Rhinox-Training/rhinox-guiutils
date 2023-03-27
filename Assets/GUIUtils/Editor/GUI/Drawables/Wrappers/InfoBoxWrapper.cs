using System;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class InfoBoxWrapper : BaseWrapperDrawable
    {
        private IPropertyMemberHelper<bool> _member;
        private string _message;
        private MessageType _type;
        private MethodInfo _getHelpIconMethod;
        private float _newHeight;

        public override float ElementHeight
        {
            get
            {
                return base.ElementHeight + _newHeight;
            }
        }

        public InfoBoxWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void DrawInner(GUIContent label)
        {
            if (_member == null || _member.GetValue())
                EditorGUILayout.HelpBox(_message, _type);
            base.DrawInner(label);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (_member == null || _member.GetValue())
            {
                _newHeight = CalcMessageHeight(rect.width);
                var helpBoxRect = rect.AlignTop(_newHeight);
                
                EditorGUI.HelpBox(helpBoxRect, _message, _type);
                rect.y += helpBoxRect.height;
                rect.height -= helpBoxRect.height;
            }

            base.DrawInner(rect, label);
        }

        private float CalcMessageHeight(float width)
        {
            if (_getHelpIconMethod == null)
            {
                _getHelpIconMethod = typeof(EditorGUIUtility).GetMethod("GetHelpIcon",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            var icon = _getHelpIconMethod.Invoke(null, new object[] {_type}) as Texture2D;
            
            return EditorStyles.helpBox.CalcHeight(
                GUIContentHelper.TempContent(_message, (Texture)icon), width);
        }

        [WrapDrawer(typeof(InfoBoxAttribute), -5000)]
        public static BaseWrapperDrawable Create(InfoBoxAttribute attr, IOrderedDrawable drawable)
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