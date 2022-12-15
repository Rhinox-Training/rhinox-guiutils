using Rhinox.Lightspeed;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Rhinox.GUIUtils.Editor
{
    public class HoverTexture
    {
        private Texture2D _tex;
        private readonly Texture2D _unhoveredTexture;

        public Texture Hovered => _tex;
        public Texture Normal => _unhoveredTexture;
            
        public HoverTexture(Texture2D tex)
            : this(tex, Color.grey)
        { }
        
        public HoverTexture(Texture2D tex, Color unhoveredColor)
        {
            _tex = tex;
            _unhoveredTexture = Utility.CopyTextureCPU(tex);
            
            var pixels = _unhoveredTexture.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] *= unhoveredColor;
            
            _unhoveredTexture.SetPixels(pixels);
            _unhoveredTexture.wrapMode = TextureWrapMode.Clamp;
            _unhoveredTexture.Apply();
        }

        private Rect _cachedRect;
        private bool _wasHovering;
        public Texture GetNeededTexture(Rect rect, out bool changed)
        {
            if (rect.width > 1)
                _cachedRect = rect;
            bool isHovering = eUtility.IsMouseOver(_cachedRect);
            changed = _wasHovering == isHovering;
            _wasHovering = isHovering;
            
            return isHovering ? Hovered : Normal;
        }
    }
}