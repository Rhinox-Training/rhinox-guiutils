using System;
using UnityEngine;

namespace Rhinox.GUIUtils.Attributes
{
    public class DrawAsTypeAttribute : PropertyAttribute
    {
        public string TypeName { get; private set; }
        public Type TargetType { get; private set; }

        public DrawAsTypeAttribute(Type type)
        {
            TargetType = type;
        }
        
        public DrawAsTypeAttribute(string type)
        {
            TypeName = type;
            TargetType = null;
        }
    }
}