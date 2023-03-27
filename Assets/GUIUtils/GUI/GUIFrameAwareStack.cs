using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.GUIUtils
{
    public class GUIFrameAwareStack<T> : Stack<T>
    {
        private NewFrameHandler _frameHandler;

        public new void Push(T t)
        {
            if (_frameHandler.IsNewFrame())
                this.Clear();
            base.Push(t);
        }

        public new T Pop()
        {
            if (this.Count != 0 && !_frameHandler.IsNewFrame())
                return base.Pop();
            Debug.LogError((object) "Pop call mismatch; no matching push call! Each Pop call must always correspond to exactly one call to Push.");
            return default (T);
        }
    }
}