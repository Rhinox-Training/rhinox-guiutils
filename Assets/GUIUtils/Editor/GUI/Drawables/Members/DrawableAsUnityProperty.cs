using System;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableAsUnityProperty : BaseMemberDrawable, IDrawableReadWrite
    {
        private Type _drawerType;
        private PropertyDrawer _drawer;
        private IHostInfoDrawer _drawerInterface;
        private float _height;
        
        public override float ElementHeight => _height;

        public DrawableAsUnityProperty(GenericHostInfo hostInfo, Type drawerType)
            : base(hostInfo)
        {
            _height = base.ElementHeight;
            _drawerType = drawerType;
            _drawer = (PropertyDrawer) Activator.CreateInstance(drawerType);
            _drawerInterface = (IHostInfoDrawer) _drawer;
            _drawerInterface.RepaintRequested += RequestRepaint;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _drawerInterface.HostInfo = _hostInfo;
            _height = _drawer.GetPropertyHeight(null, label);
            var position = GUILayoutUtility.GetRect(0, _height);
            _drawer.OnGUI(position, null, label);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            _drawerInterface.HostInfo = _hostInfo;
            _drawer.OnGUI(rect, null, label);
            _height = _drawer.GetPropertyHeight(null, label);
        }
        
        
        public object GetValue()
        {
            return _hostInfo.GetValue();
        }

        public bool TrySetValue(object value)
        {
            return _hostInfo.TrySetValue(value);
        }
    }
}