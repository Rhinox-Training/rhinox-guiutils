using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableHelpBox : BaseEntityDrawable<string>
    {
        public MessageType MsgType { get; }
        
        public DrawableHelpBox(string helpMessage, MessageType type, FieldInfo fieldInfo = null) : base(helpMessage, fieldInfo)
        {
            MsgType = type;
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout.HelpBox(Entity, MsgType);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            EditorGUI.HelpBox(rect, Entity, MsgType);
        }
    }
}