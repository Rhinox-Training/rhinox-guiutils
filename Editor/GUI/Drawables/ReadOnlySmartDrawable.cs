using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ReadOnlySmartDrawable : SimpleUnityDrawable
    {
        private readonly FieldInfo _info;
        private ICollection<IOrderedDrawable> _drawable;

        public ReadOnlySmartDrawable(SerializedObject obj, FieldInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
            _drawable = DrawableFactory.CreateDrawableMembersFor(_info.GetValue(obj.targetObject), _info.GetReturnType());
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
            if (_info.FieldType == typeof(string))
            {
                EditorGUI.TextField(rect, _info.GetValue(target) as string);
            }
            else if (_info.FieldType == typeof(int))
            {
                EditorGUI.IntField(rect, (int)_info.GetValue(target));
            }
            else if (_info.FieldType == typeof(float))
            {
                EditorGUI.FloatField(rect, (float)_info.GetValue(target));
            }
            else if (_info.FieldType == typeof(bool))
            {
                EditorGUI.Toggle(rect, (bool)_info.GetValue(target));
            }
            else if (_info.FieldType == typeof(UnityEngine.Object))
            {
                EditorGUI.ObjectField(rect, _info.GetValue(target) as UnityEngine.Object, _info.FieldType, true);
            }
            else
            {
                EditorGUI.TextField(rect, _info.GetValue(target).ToString());
            }
            EditorGUI.EndDisabledGroup();
        }
    }
    
    public class ReadOnlySmartPropertyDrawable : SimpleUnityDrawable
    {
        private readonly PropertyInfo _info;
        private ICollection<IOrderedDrawable> _drawable;

        public ReadOnlySmartPropertyDrawable(SerializedObject obj, PropertyInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
            _drawable = DrawableFactory.CreateDrawableMembersFor(_info.GetValue(obj.targetObject), _info.GetReturnType());
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