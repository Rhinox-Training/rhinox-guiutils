using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class ImageEditorIcon : EditorIcon
    {
        private static readonly string iconShader =
            "\r\nShader \"Hidden/Sirenix/Editor/GUIIcon\"\r\n{\r\n\tProperties\r\n\t{\r\n        _MainTex(\"Texture\", 2D) = \"white\" {}\r\n        _Color(\"Color\", Color) = (1,1,1,1)\r\n\t}\r\n    SubShader\r\n\t{\r\n        Blend SrcAlpha Zero\r\n        Pass\r\n        {\r\n            CGPROGRAM\r\n                #pragma vertex vert\r\n                #pragma fragment frag\r\n                #include \"UnityCG.cginc\"\r\n\r\n                struct appdata\r\n                {\r\n                    float4 vertex : POSITION;\r\n\t\t\t\t\tfloat2 uv : TEXCOORD0;\r\n\t\t\t\t};\r\n\r\n                struct v2f\r\n                {\r\n                    float2 uv : TEXCOORD0;\r\n\t\t\t\t\tfloat4 vertex : SV_POSITION;\r\n\t\t\t\t};\r\n\r\n                sampler2D _MainTex;\r\n                float4 _Color;\r\n\r\n                v2f vert(appdata v)\r\n                {\r\n                    v2f o;\r\n                    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);\r\n                    o.uv = v.uv;\r\n                    return o;\r\n                }\r\n\r\n                fixed4 frag(v2f i) : SV_Target\r\n\t\t\t\t{\r\n                    // drop shadow:\r\n                    // float texelSize = 1.0 / 34.0;\r\n                    // float2 shadowUv = clamp(i.uv + float2(-texelSize, texelSize * 2), float2(0, 0), float2(1, 1));\r\n                    // fixed4 shadow = fixed4(0, 0, 0, tex2D(_MainTex, shadowUv).a); \r\n\r\n\t\t\t\t\tfixed4 col = _Color;\r\n\t\t\t\t\tcol.a *= tex2D(_MainTex, i.uv).a;\r\n\r\n                    // drop shadow:\r\n                    // col = lerp(shadow, col, col.a);\r\n\r\n\t\t\t\t\treturn col;\r\n\t\t\t\t}\r\n\t\t\tENDCG\r\n\t\t}\r\n\t}\r\n}\r\n";

        private static Color inactiveColorPro = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static Color activeColorPro = new Color(0.55f, 0.55f, 0.55f, 1f);
        private static Color highlightedColorPro = new Color(0.9f, 0.9f, 0.9f, 1f);
        private static Color inactiveColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        private static Color activeColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static Color highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        public static Color InactiveColor => EditorGUIUtility.isProSkin ? inactiveColorPro : inactiveColor;
        public static Color ActiveColor => EditorGUIUtility.isProSkin ? activeColorPro : activeColor;
        public static Color HighlightedColor => EditorGUIUtility.isProSkin ? highlightedColorPro : highlightedColor;

        private static Material iconMat;
        private Texture2D icon;
        private Texture inactive;
        private Texture active;
        private Texture highlighted;
        private int width;
        private int height;

        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private const int DefaultSize = 34;

        /// <summary>Loads an EditorIcon from the spritesheet.</summary>
        public ImageEditorIcon(int width, int height, Texture2D tex)
        {
            this.width = width;
            this.height = height;
            this.icon = tex;
        }

        public static Texture2D ToTexture2D(Texture texture)
        {
            if (texture == null) return null;
            return Texture2D.CreateExternalTexture(
                texture.width,
                texture.height,
                TextureFormat.RGB24,
                false, false,
                texture.GetNativeTexturePtr());
        }

        public ImageEditorIcon(int width, int height, Texture tex) : this(width, height, ToTexture2D(tex))
        {
        }

        public ImageEditorIcon(Texture tex) : this(DefaultSize, DefaultSize, tex)
        {
        }

        public ImageEditorIcon(Texture2D tex) : this(DefaultSize, DefaultSize, tex)
        {
        }

        public ImageEditorIcon(int width, int height, string assetIconTex, string ext = ".png")
        {
            this.width = width;
            this.height = height;
            this.icon = UnityIcon.AssetIcon(assetIconTex, ext);
        }

        public ImageEditorIcon(string assetIconTex, string ext = ".png") : this(DefaultSize, DefaultSize, assetIconTex, ext)
        {
        }

        /// <summary>Gets the icon's highlight texture.</summary>
        public override Texture Highlighted
        {
            get
            {
                if (highlighted == null)
                    highlighted = RenderIcon(HighlightedColor);
                return highlighted;
            }
        }

        /// <summary>Gets the icon's active texture.</summary>
        public override Texture Active
        {
            get
            {
                if (active == null)
                    active = RenderIcon(ActiveColor);
                return active;
            }
        }

        /// <summary>Gets the icon's inactive texture.</summary>
        public override Texture Inactive
        {
            get
            {
                if (inactive == null)
                    inactive = RenderIcon(InactiveColor);
                return inactive;
            }
        }

        /// <summary>Not yet documented.</summary>
        public override Texture2D Raw => icon;

        private Texture RenderIcon(Color color)
        {
            if (iconMat == null || iconMat.shader == null)
                iconMat = new Material(ShaderUtil.CreateShaderAsset(iconShader));

            iconMat.SetColor(ColorID, color);

            bool sRgbWrite = GL.sRGBWrite;
            GL.sRGBWrite = true;

            RenderTexture active = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(this.width, this.height, 0);
            RenderTexture.active = temporary;

            GL.Clear(false, true, new Color(1f, 1f, 1f, 0.0f));
            Graphics.Blit(this.Raw, temporary, iconMat);
            Texture2D texture2D = new Texture2D(temporary.width, temporary.height, TextureFormat.ARGB32, false, true);

            texture2D.filterMode = FilterMode.Bilinear;
            texture2D.ReadPixels(new Rect(0.0f, 0.0f, temporary.width, temporary.height), 0, 0);
            texture2D.alphaIsTransparency = true;
            texture2D.Apply();

            RenderTexture.ReleaseTemporary(temporary);
            RenderTexture.active = active;
            GL.sRGBWrite = sRgbWrite;
            return texture2D;
        }
    }
}