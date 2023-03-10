using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableHelpBox : BaseEntityDrawable
    {
        public MessageType MsgType { get; }
        
        public DrawableHelpBox(string obj, MessageType type, FieldInfo fieldInfo = null) : base(obj, fieldInfo)
        {
            MsgType = type;
        }
        
        protected override void Draw(object target)
        {
            EditorGUILayout.HelpBox((string)target, MsgType);
        }

        protected override void Draw(Rect rect, object target)
        {
            EditorGUI.HelpBox(rect, (string)target, MsgType);
        }
    }
}