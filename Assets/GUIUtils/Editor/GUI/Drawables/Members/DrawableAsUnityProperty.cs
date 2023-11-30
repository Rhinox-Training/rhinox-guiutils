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

            if (_drawerType.IsGenericType && _drawerType.IsGenericTypeDefinition)
            {
                var hostType = hostInfo.GetReturnType();
                var arguments = hostType.GetGenericArguments();
                while (arguments.Length == 0)
                {
                    hostType = hostType.BaseType;
                    if (hostType == null)
                        break;
                    arguments = hostType.GetGenericArguments();
                }
                
                if (arguments.Length > 0)
                    _drawerType = _drawerType.MakeGenericType(arguments);
            }

            _drawer = (PropertyDrawer) Activator.CreateInstance(_drawerType);
            _drawerInterface = (IHostInfoDrawer) _drawer;
            if (_drawer is IRepaintEvent repaintEventHandler)
                repaintEventHandler.RepaintRequested += RequestRepaint;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            _drawerInterface.SetupForHostInfo(_hostInfo, string.Empty);
            _height = _drawer.GetPropertyHeight(null, label);
            var position = GUILayoutUtility.GetRect(0, _height);
            _drawer.OnGUI(position, null, label);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            _drawerInterface.SetupForHostInfo(_hostInfo, string.Empty);
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