using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public interface IDrawableMember
    {
        object Draw(object target);
    }
}