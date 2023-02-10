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
        private readonly ICollection<IOrderedDrawable> _drawables;

        public DrawablePropertyView(object nonUnityObjInstance)
        {
            if (nonUnityObjInstance == null) throw new ArgumentNullException(nameof(nonUnityObjInstance));
            _instance = nonUnityObjInstance;
            _drawables = DrawableFactory.ParseNonUnityObject(nonUnityObjInstance);
        }
        
        public DrawablePropertyView(SerializedObject unityObjInstance)
        {
            if (unityObjInstance == null) throw new ArgumentNullException(nameof(unityObjInstance));
            _instance = unityObjInstance;
            _drawables = DrawableFactory.ParseSerializedObject(unityObjInstance);
        }
        
        public void DrawLayout()
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw();
            }
        }
        
        public void Draw(Rect rect)
        {
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw(rect);
            }
        }
    }
}