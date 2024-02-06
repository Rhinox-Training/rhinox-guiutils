using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Allows you to create Unity Editors for target objects.
    /// This was mostly abandoned in favour of DrawableFactory.
    /// </summary>
    public static class EditorCreator
    {
        private static PropertyInfo s_materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.FlattenHierarchy);
        
        public static IEditor CreateEditorForTarget(object obj)
        {
            if (obj is EditorWindow editorWindow)
                return TryCreateGenericEditor(editorWindow);

            if (obj is Object targetObject)
            {
                var curEditor = CreateStandardEditor(targetObject);
                if (curEditor == null)
                    curEditor = TryCreateGenericEditor(targetObject);

                return curEditor;
            }

            return TryCreateGenericNonUnityEditor(obj);
        }
        
        private static IEditor TryCreateGenericNonUnityEditor(object systemObj)
        {
            try
            {
                return GenericSmartObjectEditor.Create(systemObj);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }

        private static IEditor CreateStandardEditor(UnityEngine.Object targetObject)
        {
            var editor = UnityEditor.Editor.CreateEditor(targetObject);
            if (editor is MaterialEditor matEditor && s_materialForceVisibleProperty != null)
                s_materialForceVisibleProperty.SetValue(matEditor, true, null);
            
            if (editor != null)
                return new UnityEditorWrapper(editor);
            
            return null;
        }
        
        private static IEditor TryCreateGenericEditor(UnityEngine.Object targetObject)
        {
            UnityEditor.Editor customEditor = null;
            try
            {
                customEditor = UnityEditor.Editor.CreateEditor(targetObject, typeof(GenericSmartUnityObjectEditor));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
            
            return new UnityEditorWrapper(customEditor);
        }
    }
}