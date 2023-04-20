using System;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IPropertyMemberHelper
    {
        void DrawError();
        void DrawError(Rect rect);

        object GetValue();
    }
    
    public interface IPropertyMemberHelper<out T> : IPropertyMemberHelper
    {
        T GetSmartValue();
        T ForceGetValue();
    }
}