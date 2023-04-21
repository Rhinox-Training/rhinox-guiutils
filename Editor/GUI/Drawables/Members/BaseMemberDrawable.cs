using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseMemberDrawable : BaseDrawable
    {
        public override string LabelString => HostInfo.NiceName;
        
        public GenericHostInfo HostInfo { get; }
        
        private Attribute[] _cachedAttributes;
        
        protected BaseMemberDrawable(GenericHostInfo hostInfo)
        {
            HostInfo = hostInfo;
            Host = hostInfo.Parent ?? hostInfo.GetHost();
        }
        
        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_cachedAttributes == null)
                _cachedAttributes = HostInfo.GetAttributes();
            
            return _cachedAttributes.OfType<TAttribute>();
        }
    }
}