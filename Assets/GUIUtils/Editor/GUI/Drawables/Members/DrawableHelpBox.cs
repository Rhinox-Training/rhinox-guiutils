using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableHelpBox : BaseMemberDrawable
    {
        public string HelpMessage { get; }
        public MessageType MsgType { get; }

        
        public DrawableHelpBox(string helpMessage, MessageType type, GenericHostInfo hostInfo) 
            : base(hostInfo)
        {
            HelpMessage = helpMessage;
            MsgType = type;
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout.HelpBox(HelpMessage, MsgType);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.HelpBox(rect, HelpMessage, MsgType);
        }
    }
}