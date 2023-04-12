using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IEditor
    {
        bool HasPreviewGUI();
        
        void DrawPreview(Rect rect);
        
        bool CanDraw();
        
        void Draw();
        
        void Destroy();
    }
}