using System;
using Rhinox.Lightspeed;

namespace Rhinox.GUIUtils.Attributes
{
    public class HostInfoTypeHintAttribute : Attribute
    {
        public string Member { get; }

        public HostInfoTypeHintAttribute(string member)
        {
            Member = member;
        }
    }
}