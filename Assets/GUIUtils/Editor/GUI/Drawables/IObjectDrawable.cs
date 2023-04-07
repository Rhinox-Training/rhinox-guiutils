using System;

namespace Rhinox.GUIUtils.Editor
{
    public interface IObjectDrawable
    {
        object Instance { get; }
        Type ObjectType { get; }
    }
}