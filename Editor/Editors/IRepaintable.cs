using System;

namespace Rhinox.GUIUtils.Editor
{
    public interface IRepaintRequestHandler
    {
        void UpdateRequestTarget(IRepaintable target);
    }
    
    public interface IRepaintable
    {
        void RequestRepaint();
    }

    public interface IRepaintEvent
    {
        event Action RepaintRequested;
    }
}