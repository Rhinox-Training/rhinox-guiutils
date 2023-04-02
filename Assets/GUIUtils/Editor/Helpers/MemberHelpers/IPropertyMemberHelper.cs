using System;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IPropertyMemberHelper<out T>
    {
        void DrawError();
        void DrawError(Rect rect);

        T GetValue();
        T ForceGetValue();
    }
}