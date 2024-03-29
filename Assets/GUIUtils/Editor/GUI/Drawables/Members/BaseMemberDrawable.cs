﻿using System;
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
        protected override string LabelString => HostInfo.NiceName;
        
        private ICollection<Attribute> _cachedAttributes;
        
        protected BaseMemberDrawable(GenericHostInfo hostInfo)
        {
            _hostInfo = hostInfo;
        }
        
        public override IEnumerable<TAttribute> GetDrawableAttributes<TAttribute>()
        {
            if (_cachedAttributes == null)
                _cachedAttributes = HostInfo.GetAttributes();
            
            return _cachedAttributes.OfType<TAttribute>();
        }
    }
}