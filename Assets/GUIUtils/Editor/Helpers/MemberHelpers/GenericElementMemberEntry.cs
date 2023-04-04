using System;
using System.Collections;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.GUIUtils.Editor
{
    public class GenericElementMemberEntry : GenericMemberEntry
    {
        public int Index;
        private Type _elementType;
        
        public GenericElementMemberEntry(GenericMemberEntry listEntry, int index)
            : base(listEntry.Instance, listEntry.Info, listEntry)
        {
            Index = index;
            _elementType = Info.GetReturnType().GetCollectionElementType();
        }

        public override Attribute[] GetAttributes() => Array.Empty<Attribute>();

        public override Type GetReturnType() => _elementType;

        public override object GetValue()
        {
            var list = (IList) Parent.GetValue();
            return list[Index];
        }

        public override bool TrySetValue<T>(T val)
        {
            var list = (IList) Parent.GetValue();
            list[Index] = val;
            return true;
        }
    }
}