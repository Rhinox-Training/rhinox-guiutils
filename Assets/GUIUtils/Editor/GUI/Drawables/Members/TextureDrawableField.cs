using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Rhinox.GUIUtils.Editor
{
    public class TextureDrawableField : BaseMemberDrawable<UnityEngine.Texture>
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
                    rect = GUILayoutUtility.GetRect(_previewAttr.Height, _previewAttr.Height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                else
                {
                    rect = GUILayoutUtility.GetRect(DEFAULT_SIZE, DEFAULT_SIZE, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                }

                DrawTexturePreview(ref memberVal, rect);
                return memberVal;
            }
            return EditorGUILayout.ObjectField(memberVal, _info.GetReturnType(), true) as Texture;
        }

        protected override UnityEngine.Texture DrawValue(Rect rect, object instance, UnityEngine.Texture memberVal)
        {
            if (_previewAttr != null)
            {
                DrawTexturePreview(ref memberVal, rect);
                return memberVal;
            }
            return EditorGUI.ObjectField(rect, memberVal, _info.GetReturnType(), true) as Texture;
        }

        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            string commandName = Event.current.commandName;
            if (commandName == "ObjectSelectorUpdated") {
                SetSmartValue(EditorGUIUtility.GetObjectPickerObject() as Texture2D);
            } else if (commandName == "ObjectSelectorClosed") {
                SetSmartValue(EditorGUIUtility.GetObjectPickerObject() as Texture2D);
            }
        }

        private void DrawTexturePreview(ref Texture memberVal, Rect rect)
        {
            Texture targetPaint = memberVal;
            if (targetPaint == null)
                targetPaint = Utility.GetColorTexture(Color.gray);
            
            EditorGUI.DrawPreviewTexture(rect, targetPaint, null, ScaleMode.ScaleToFit);

            var isReadOnly = CheckIfIsReadOnly();

            if (!isReadOnly)
            {
                rect = rect.AlignBottom(rect.height / 5.0f);
                rect = rect.AlignCenter(rect.width * 0.6f);
                if (GUI.Button(rect, "Select"))
                {
                    int controlID = EditorGUIUtility.GetControlID (FocusType.Passive);
                    EditorGUIUtility.ShowObjectPicker<Texture2D> (memberVal, true, "", controlID);
                }
            }
        }

        private bool CheckIfIsReadOnly()
        {
            bool isReadOnly = false;
            if (this._info is PropertyInfo propertyInfo)
            {
                isReadOnly = propertyInfo.GetSetMethod(false) == null;
            }

            if (!isReadOnly)
                isReadOnly = _info.GetCustomAttribute<ReadOnlyAttribute>() != null;
            return isReadOnly;
        }
    }
}