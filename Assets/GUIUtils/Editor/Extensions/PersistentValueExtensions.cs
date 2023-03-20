using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public static class PersistentValueExtensions
    {
        public static bool ShowField(this PersistentValue<bool> val, string label)
        {
            var result = EditorGUILayout.Toggle(label, val);
            return val.Set(result);
        }
        
        public static bool ShowField(this PersistentValue<float> val, string label)
        {
            var result = EditorGUILayout.FloatField(label, val);
            return val.Set(result);
        }
    }
}