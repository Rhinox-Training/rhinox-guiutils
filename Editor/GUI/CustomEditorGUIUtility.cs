using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public class CustomEditorGUIUtility
    {
        public static void SelectObject(Object obj)
        {
            if (obj == null)
                return;
            
            if (AssetDatabase.Contains(obj) && !AssetDatabase.IsMainAsset(obj))
            {
                Object o = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(obj));
                if (o is Component)
                    o = (o as Component).gameObject;
                Selection.activeObject = o;
            }
            else
            {
                if (obj is Component)
                    obj = (obj as Component).gameObject;
                Selection.activeObject = obj;
            }
        }
    }
}