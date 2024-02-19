using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(DrawAsTypeAttribute))]
    public class DrawAsTypePropertyDrawer : BasePropertyDrawer<object>
    {
        private Type _type;
        private string _cachedTypeName;

        private IPropertyMemberHelper<string> _typeHelper;
        private string _errorMessage;

        private IOrderedDrawable _drawable;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var attr = HostInfo.GetAttribute<DrawAsTypeAttribute>();
            
            if (attr.TargetType != null)
                _type = attr.TargetType;
            else
            {
                _type = typeof(object);
                _typeHelper = MemberHelper.Create<string>(HostInfo, attr.TypeName);
            }
        }

        
        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            
            var type = GetTargetType();

            if (_drawable == null)
                _drawable = DrawableFactory.CreateDrawableFor(data, type);
            
            _drawable.Draw(label);
            
            if (!EditorGUI.EndChangeCheck())
                return;
        }

        private Type GetTargetType()
        {
            if (_typeHelper == null)
                return _type;
            
            var typeName = _typeHelper.GetSmartValue();
            if (typeName == _cachedTypeName)
                return _type;
            
            _cachedTypeName = typeName;
            _type = ReflectionUtility.FindTypeExtensively(ref typeName);
            return _type;
        }

    }
}