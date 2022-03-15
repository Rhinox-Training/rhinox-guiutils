using System;

namespace Rhinox.GUIUtils.Attributes
{
    public class GenericValueAttribute : Attribute
    {
        public string TypeName { get; private set; }
        public Type TargetType { get; private set; }

        public GenericValueAttribute(Type type)
        {
            TargetType = type;
        }
        
        public GenericValueAttribute(string type)
        {
            TypeName = type;
            TargetType = null;
        }
    }
}