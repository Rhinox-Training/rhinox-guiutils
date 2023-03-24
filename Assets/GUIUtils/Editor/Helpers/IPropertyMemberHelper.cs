using System;

namespace Rhinox.GUIUtils.Editor
{
    public interface IPropertyMemberHelper<out T>
    {
        Type ObjectType { get; }
        
        T GetValue();
        T ForceGetValue();
    }
}