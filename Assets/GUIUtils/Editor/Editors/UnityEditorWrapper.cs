using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class UnityEditorWrapper : IEditor
    {
        public UnityEditor.Editor UnityEditor;

        public UnityEditorWrapper(UnityEditor.Editor editor)
        {
            UnityEditor = editor;
        }
        
        public bool HasPreviewGUI() => UnityEditor.HasPreviewGUI();

        public void DrawPreview(Rect rect) => UnityEditor.DrawPreview(rect);
        
        public bool CanDraw() => UnityEditor.target != null;


        public void Draw()
        {
            UnityEditor.OnInspectorGUI();
        }

        public void Destroy()
        {
            Object.DestroyImmediate(UnityEditor);
        }
    }
}