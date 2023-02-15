using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class TextureDrawableField : BaseDrawable<UnityEngine.Texture>
    {
        private readonly PreviewFieldAttribute _previewAttr;
        private const int DEFAULT_SIZE = 64;

        public TextureDrawableField(object instance, MemberInfo info) : base(instance, info)
        {
            _previewAttr = info.GetCustomAttribute<PreviewFieldAttribute>();
        }
        
        protected override UnityEngine.Texture DrawValue(object instance, UnityEngine.Texture memberVal)
        {
            if (_previewAttr != null)
            {
                Rect rect = default;
                
                if (_previewAttr.Height > 0.0f)
                    rect = GUILayoutUtility.GetRect(_previewAttr.Height * memberVal.width / memberVal.height, _previewAttr.Height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                else
                {
                    float width = DEFAULT_SIZE * memberVal.width / memberVal.height;
                    rect = GUILayoutUtility.GetRect(width, DEFAULT_SIZE, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                }

                EditorGUI.DrawPreviewTexture(rect, memberVal);
                return memberVal;
            }
            return EditorGUILayout.ObjectField(memberVal, _info.GetReturnType(), true) as Texture;
        }

        protected override UnityEngine.Texture DrawValue(Rect rect, object instance, UnityEngine.Texture memberVal)
        {
            if (_previewAttr != null)
            {
                EditorGUI.DrawPreviewTexture(rect, memberVal);
                return memberVal;
            }
            return EditorGUI.ObjectField(rect, memberVal, _info.GetReturnType(), true) as Texture;
        }
    }
}