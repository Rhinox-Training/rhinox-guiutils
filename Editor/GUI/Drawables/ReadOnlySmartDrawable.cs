using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ReadOnlySmartDrawable : SimpleDrawable
    {
        private readonly FieldInfo _info;

        public ReadOnlySmartDrawable(SerializedObject obj, FieldInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
        }

        protected override void Draw(Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            if (_info.FieldType == typeof(string))
            {
                EditorGUILayout.TextField(_info.GetValue(target) as string);
            }
            else if (_info.FieldType == typeof(int))
            {
                EditorGUILayout.IntField((int)_info.GetValue(target));
            }
            else if (_info.FieldType == typeof(float))
            {
                EditorGUILayout.FloatField((float)_info.GetValue(target));
            }
            else if (_info.FieldType == typeof(bool))
            {
                EditorGUILayout.Toggle((bool)_info.GetValue(target));
            }
            else if (_info.FieldType == typeof(UnityEngine.Object))
            {
                EditorGUILayout.ObjectField(_info.GetValue(target) as UnityEngine.Object, _info.FieldType);
            }
            else
            {
                EditorGUILayout.TextField(_info.GetValue(target).ToString());
            }
            EditorGUI.EndDisabledGroup();
        }
    }
    
    public class ReadOnlySmartPropertyDrawable : SimpleDrawable
    {
        private readonly PropertyInfo _info;

        public ReadOnlySmartPropertyDrawable(SerializedObject obj, PropertyInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
        }

        protected override void Draw(Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            if (_info.PropertyType == typeof(string))
            {
                EditorGUILayout.TextField(_info.GetValue(target) as string);
            }
            else if (_info.PropertyType == typeof(int))
            {
                EditorGUILayout.IntField((int)_info.GetValue(target));
            }
            else if (_info.PropertyType == typeof(float))
            {
                EditorGUILayout.FloatField((float)_info.GetValue(target));
            }
            else if (_info.PropertyType == typeof(bool))
            {
                EditorGUILayout.Toggle((bool)_info.GetValue(target));
            }
            else if (_info.PropertyType == typeof(Object))
            {
                EditorGUILayout.ObjectField(_info.GetValue(target) as Object, _info.PropertyType);
            }
            else
            {
                EditorGUILayout.TextField(_info.GetValue(target).ToString());
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}