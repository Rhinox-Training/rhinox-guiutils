using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class IntDrawableField : BaseMemberValueDrawable<int>
    {
        private int? _min;
        private int? _max;

        public IntDrawableField(GenericHostInfo hostInfo) : base(hostInfo) { }

        protected override void Initialize()
        {
            base.Initialize();

            if (TryGetDrawableAttribute(out MinValueAttribute minAttr))
                _min = (int) Math.Round(minAttr.MinValue);
            if (TryGetDrawableAttribute(out MaxValueAttribute maxAttr))
                _max = (int) Math.Round(maxAttr.MaxValue);
        }

        protected override void PostProcessValue(ref int value)
        {
            if (_min.HasValue && value < _min)
                value = _min.Value;
            if (_max.HasValue && value > _max)
                value = _max.Value;
            
            base.PostProcessValue(ref value);
        }

        protected override int DrawValue(GUIContent label, int memberVal, params GUILayoutOption[] options)
        {
            return EditorGUILayout.IntField(label, memberVal, CustomGUIStyles.CleanTextField, options);
        }

        protected override int DrawValue(Rect rect, GUIContent label, int memberVal)
        {
            return EditorGUI.IntField(rect, label, memberVal);
        }
    }
}