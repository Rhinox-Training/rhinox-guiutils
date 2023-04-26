using System;
using System.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public static class CustomPropertyDrawerAttributeExtensions
    {
        private static FieldInfo _typeMember;
        private static FieldInfo _useForChildrenMember;
        
        public static Type GetPropertyType(this CustomPropertyDrawer drawerAttribute)
        {
            if (_typeMember == null)
                _typeMember = typeof(CustomPropertyDrawer).GetField("m_Type",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            return (Type) _typeMember.GetValue(drawerAttribute);
        }

        public static bool IsUsedForChildren(this CustomPropertyDrawer drawerAttribute)
        {
            if (_useForChildrenMember == null)
                _useForChildrenMember = typeof(CustomPropertyDrawer).GetField("m_UseForChildren",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return (bool) _useForChildrenMember.GetValue(drawerAttribute);
        }
    }
}