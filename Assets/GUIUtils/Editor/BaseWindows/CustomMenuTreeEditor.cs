using System.Net;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace GUIUtils.Editor.BaseWindows
{
    [CustomEditor(typeof(CustomMenuTree))]
    public class CustomMenuTreeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets != null)
            {
                foreach (var targ in this.targets)
                {
                    if (!(targ is CustomMenuTree menuTreeTarget))
                        continue;
                    menuTreeTarget.Draw(Event.current);
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }
    }
}