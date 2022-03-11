using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Odin
{
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class InlineEditorIconAttribute : Attribute
    {
        public string EditorIcon { get; private set; }
        public string MemberMethod { get; private set; }
        
        public string Tooltip { get; set; }
        
        public InlineEditorIconAttribute(string memberMethod, string editorIcon)
        {
            this.MemberMethod = memberMethod;
            this.EditorIcon = editorIcon;
        }
    }
}