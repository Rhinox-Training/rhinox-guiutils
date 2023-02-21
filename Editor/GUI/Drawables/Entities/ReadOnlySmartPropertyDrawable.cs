using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ReadOnlySmartPropertyDrawable : BaseUnitySerializedDrawable
    {
        private readonly PropertyInfo _info;
        private ICollection<IOrderedDrawable> _drawable;

        public ReadOnlySmartPropertyDrawable(SerializedObject obj, PropertyInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
            _drawable = DrawableFactory.ParseNonUnityObject(_info.GetValue(obj.targetObject));
        }

        protected override void Draw(Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            foreach (var draw in _drawable)
                draw.Draw();
            EditorGUI.EndDisabledGroup();
        }

        protected override void Draw(Rect rect, Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            if (_info.PropertyType == typeof(string))
            {
                EditorGUI.TextField(rect, _info.GetValue(target) as string);
            }
            else if (_info.PropertyType == typeof(int))
            {
                EditorGUI.IntField(rect, (int)_info.GetValue(target));
            }
            else if (_info.PropertyType == typeof(float))
            {
                EditorGUI.FloatField(rect, (float)_info.GetValue(target));
            }
            else if (_info.PropertyType == typeof(bool))
            {
                EditorGUI.Toggle(rect, (bool)_info.GetValue(target));
            }
            else if (_info.PropertyType == typeof(Object))
            {
                EditorGUI.ObjectField(rect, _info.GetValue(target) as Object, _info.PropertyType, true);
            }
            else
            {
                EditorGUI.TextField(rect, _info.GetValue(target).ToString());
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}