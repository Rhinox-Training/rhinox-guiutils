using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HoverRect
    {
        public Color? HoverColor;
        public Color? ClickColor;
        
        protected Rect _cachedRect;
        protected bool _wasHovering;
        protected bool _isClicked;

        private bool _pushedColor;


        public bool IsHovering(Rect rect, out bool changed)
        {
            if (rect.width > 1)
                _cachedRect = rect;

            var eType = Event.current.type;
            if (eType == EventType.MouseDown)
                _isClicked = true;
            else if (eType == EventType.MouseUp)
                _isClicked = false;
            
            if (eType != EventType.Repaint)
            {
                bool isHovering = eUtility.IsMouseOver(_cachedRect);
                changed = _wasHovering == isHovering;
                _wasHovering = isHovering;
            }
            else changed = false;
            
            return _wasHovering;
        }
        
        public void PushColor(Rect rect)
        {
            IsHovering(rect, out _);
            PushColor();
        }

        public void PushColor()
        {
            if (_pushedColor)
                PopColor();

            if (!_wasHovering) return;

            var color = _isClicked ? ClickColor : HoverColor;
            if (!color.HasValue) return;
            
            GUIContentHelper.PushColor(color.Value);
            _pushedColor = true;
        }

        public void PopColor()
        {
            if (!_pushedColor) return;
            
            GUIContentHelper.PopColor();
            _pushedColor = false;
        }
    }
}