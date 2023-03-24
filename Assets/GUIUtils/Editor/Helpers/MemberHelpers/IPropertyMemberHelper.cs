using System;

namespace Rhinox.GUIUtils.Editor
{
    public interface IPropertyMemberHelper<out T>
    {
        T GetValue();
        T ForceGetValue();
    }
}