using System;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class HoverTexture : HoverRect
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
            
            // TODO GUI.color also affects texture so is this still even needed?
            var pixels = _unhoveredTexture.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] *= unhoveredColor;
            
            _unhoveredTexture.SetPixels(pixels);
            _unhoveredTexture.wrapMode = TextureWrapMode.Clamp;
            _unhoveredTexture.Apply();
        }
        
        public Texture GetNeededTexture(Rect rect, out bool changed) => IsHovering(rect, out changed) ? Hovered : Normal;

        public void Draw(Rect rect)
        {
            var tex = GetNeededTexture(rect, out _);
            if (!GUI.enabled)
                tex = Normal;
            GUI.DrawTexture(rect, tex);
        }
    }
}