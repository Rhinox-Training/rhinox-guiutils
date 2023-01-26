using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface ISimpleDrawable
    {
        int Order { get; set; }
        void Draw();
        void Draw(Rect rect);
    }
}