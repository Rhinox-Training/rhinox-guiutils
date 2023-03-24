using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Checks for whether there is a new frame; To do this properly it needs to called during a repaint
    /// This cannot be used when it isn't consistently called in repaint
    /// </summary>
    public struct NewFrameHandler
    {
        private bool _isNewFrame, _nextEventIsNew;
        
        public bool IsNewFrame()
        {
            if (Event.current == null)
                return _isNewFrame;
            EventType type = Event.current.type;
            if (type == EventType.Repaint)
            {
                _nextEventIsNew = true;
                _isNewFrame = false;
                return _isNewFrame;
            }
            
            if (_nextEventIsNew)
            {
                _nextEventIsNew = false;
                _isNewFrame = true;
                return _isNewFrame;
            }
            _isNewFrame = false;
            return _isNewFrame;
        }
    }
}