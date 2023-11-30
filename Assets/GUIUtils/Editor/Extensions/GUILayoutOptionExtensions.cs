using System.Reflection;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class GUILayoutOptionExtensions
    {
        private static FieldInfo _typeField;

        public static int GetLayoutType(this GUILayoutOption option)
        {
            if (_typeField == null)
                _typeField = typeof(GUILayoutOption).GetField("type");

            return (int)_typeField.GetValue(option);
        }

        public static bool IsWidth(this GUILayoutOption option)
        {
            int layoutType = GetLayoutType(option);
            return layoutType.EqualsOneOf(0, 2, 3, 6);
        }

        public static bool IsHeight(this GUILayoutOption option)
        {
            int layoutType = GetLayoutType(option);
            return layoutType.EqualsOneOf(1, 4, 5, 7);
        }
        
        /*
         *  internal enum Type
            {
              fixedWidth, = 0
              fixedHeight, = 1
              minWidth, = 2
              maxWidth, = 3
              minHeight, = 4
              maxHeight, = 5
              stretchWidth, = 6
              stretchHeight, = 7
              alignStart, = 8
              alignMiddle, = 9
              alignEnd, = 10
              alignJustify, = 11
              equalSize, = 12
              spacing, = 13
            }
         */
    }
}