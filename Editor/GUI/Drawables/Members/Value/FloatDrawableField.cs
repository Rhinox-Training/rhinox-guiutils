using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class FloatDrawableField : BaseMemberValueDrawable<float>
    {
        private float? _min;
        private float? _max;

        public FloatDrawableField(GenericHostInfo hostInfo) : base(hostInfo) { }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (TryGetDrawableAttribute(out MinValueAttribute minAttr))
                _min = (float) minAttr.MinValue;
            if (TryGetDrawableAttribute(out MaxValueAttribute maxAttr))
                _max = (float) maxAttr.MaxValue;
        }

        protected override void PostProcessValue(ref float value)
        {
            if (_min.HasValue && value < _min)
                value = _min.Value;
            if (_max.HasValue && value > _max)
                value = _max.Value;
            
            base.PostProcessValue(ref value);
        }
        
        protected override float DrawValue(GUIContent label, float memberVal, params GUILayoutOption[] options)
        {
            return EditorGUILayout.FloatField(label, memberVal, CustomGUIStyles.CleanTextField, options);
        }

        protected override float DrawValue(Rect rect, GUIContent label, float memberVal)
        {
            return EditorGUI.FloatField(rect, label, memberVal);
        }
    }
}