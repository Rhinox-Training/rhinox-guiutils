using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawablePropertyView
    {
        private readonly object _instance;
        private readonly ICollection<IDrawableMember> _drawables;
        private readonly ICollection<IOrderedDrawable> _additionalDrawables;

        public DrawablePropertyView(object nonUnityObjInstance)
        {
            if (nonUnityObjInstance == null) throw new ArgumentNullException(nameof(nonUnityObjInstance));
            _instance = nonUnityObjInstance;
            _drawables = DrawableMemberFactory.CreateDrawableMembersFor(_instance.GetType());
            _additionalDrawables = DrawableFactory.ParseNonUnityObject(nonUnityObjInstance);
        }
        
        public void Draw()
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw(_instance);
            }
            
            foreach (var drawable in _additionalDrawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw();
            }
        
        }
    }
}