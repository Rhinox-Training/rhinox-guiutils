using System;
using System.Collections;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public class GenericElementMemberEntry : GenericMemberEntry
    {
        public int Index;
        private Type _elementType;
        private readonly GenericHostInfo _hostInfo;

        public GenericElementMemberEntry(GenericMemberEntry listEntry, int index)
            : base(listEntry.Instance, listEntry.Info, listEntry)
        {
            Index = index;
            _hostInfo = new GenericHostInfo(listEntry.Instance, listEntry.Info as FieldInfo, Index);
            _elementType = Info.GetReturnType().GetCollectionElementType();
        }

        public override Attribute[] GetAttributes() => new []{ new HideLabelAttribute()};

        public override Type GetReturnType() => _elementType;

        public override object GetValue()
        {
            if (_hostInfo != null)
                return _hostInfo.GetValue();
            return base.GetValue();
        }

        public override bool TrySetValue(object val)
        {
            if (_hostInfo != null)
            {
                _hostInfo.SetValue(val);
                return true;
            }

            return base.TrySetValue(val);
        }
    }
}