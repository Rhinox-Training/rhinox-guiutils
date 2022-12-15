using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface ISimpleDrawable
    {
        void Draw();
        void Draw(Rect rect);
    }
}