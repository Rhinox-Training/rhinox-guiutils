using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public struct ValidRect
    {
        private Rect _rect;

        public float x => _rect.x;
        
        public float y => _rect.y;
        public float width => _rect.width;
        public float height => _rect.height;

        /// <summary>
        /// Returns whether the ValidRect was updated.
        /// This only happens when the given rect is valid and differs from the cache
        /// </summary>
        public bool Update(Rect rect)
        {
            if (!rect.IsValid() || _rect == rect)
                return false;
            
            _rect = rect;
            return true;
        }

        public bool IsValid() => _rect.IsValid();

        public static implicit operator Rect(ValidRect r) => r._rect;
    }
}