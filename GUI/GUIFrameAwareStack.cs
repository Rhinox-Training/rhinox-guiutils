using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public class GUIFrameAwareStack<T> : Stack<T>
    {
        private bool _isNewFrame = true;
        private bool _nextEventIsNew = true;

        public new void Push(T t)
        {
            if (IsNewFrame())
                this.Clear();
            base.Push(t);
        }

        public new T Pop()
        {
            if (this.Count != 0 && !IsNewFrame())
                return base.Pop();
            Debug.LogError((object) "Pop call mismatch; no matching push call! Each Pop call must always correspond to exactly one call to Push.");
            return default (T);
        }
        
        private bool IsNewFrame()
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