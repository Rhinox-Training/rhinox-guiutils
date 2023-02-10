using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IOrderedDrawable
    {
        int Order { get; set; }
        void Draw();
        void Draw(Rect rect);
    }
}