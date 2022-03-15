using System;

namespace Rhinox.GUIUtils.Attributes
{
    public class ToggledByAttribute : Attribute
    {
        public string ToggleMember;
        public bool MakeReadOnly = true;

        public ToggledByAttribute(string member)
        {
            ToggleMember = member;
        }
    }
}
