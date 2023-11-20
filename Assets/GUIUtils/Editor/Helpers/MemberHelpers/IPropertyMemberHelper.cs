using System;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IPropertyMemberHelper
    {
        bool HasError { get; }
        bool DrawError();
        bool DrawError(Rect rect);

        object GetValue();
    }
    
    public interface IPropertyMemberHelper<out T> : IPropertyMemberHelper
    {
        T GetSmartValue();
        T ForceGetValue();
    }
}