using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class PersistentValueExtensions
    {
        public static bool ShowInternalIcon(this PersistentValue<bool> val, string icon, string tooltip)
        {
            bool result = CustomEditorGUI.IconToggle(UnityIcon.InternalIcon(icon), val, tooltip: tooltip);
            return val.Set(result);
        }
        
        public static bool ShowField(this PersistentValue<bool> val, string label, params GUILayoutOption[] layoutOptions)
        {
            var result = EditorGUILayout.Toggle(label, val, layoutOptions);
            return val.Set(result);
        }
        
        public static bool ShowField(this PersistentValue<float> val, string label, params GUILayoutOption[] layoutOptions)
        {
            var result = EditorGUILayout.FloatField(label, val, layoutOptions);
            return val.Set(result);
        }
        
        public static bool ShowField(this PersistentValue<int> val, string label, params GUILayoutOption[] layoutOptions)
        {
            var result = EditorGUILayout.IntField(label, val, layoutOptions);
            return val.Set(result);
        }
        
        public static bool ShowField(this PersistentValue<string> val, string label, params GUILayoutOption[] layoutOptions)
        {
            var result = EditorGUILayout.TextField(label, val, layoutOptions);
            return val.Set(result);
        }
    }
}