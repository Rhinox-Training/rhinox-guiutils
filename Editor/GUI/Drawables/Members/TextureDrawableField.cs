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

        private int _activeControlId;

        public override float ElementHeight
        {
            get
            {
                if (_previewAttr == null)
                    return base.ElementHeight;
                return _previewAttr.Height + 2.0f; // 2 x padding
            }
        }

        public TextureDrawableField(GenericHostInfo hostInfo) : base(hostInfo)
        {
            _previewAttr = hostInfo.GetAttribute<PreviewFieldAttribute>();
        }
        
        protected override UnityEngine.Texture DrawValue(GUIContent label, UnityEngine.Texture memberVal, params GUILayoutOption[] options)
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
            return EditorGUILayout.ObjectField(memberVal, HostInfo.GetReturnType(), true, options) as Texture;
        }

        protected override UnityEngine.Texture DrawValue(Rect rect, GUIContent label, UnityEngine.Texture memberVal)
        {
            if (_previewAttr != null)
            {
                DrawTexturePreview(ref memberVal, rect);
                return memberVal;
            }
            return EditorGUI.ObjectField(rect, memberVal, HostInfo.GetReturnType(), true) as Texture;
        }

        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            string commandName = Event.current.commandName;
            
            if (EditorGUIUtility.GetObjectPickerControlID() != _activeControlId)
                return;
            
            if (commandName == "ObjectSelectorUpdated")
            {
                var picker = EditorGUIUtility.GetObjectPickerObject();
                SetSmartValue(picker as Texture2D);
            }
            else if (commandName == "ObjectSelectorClosed")
            {
                var picker = EditorGUIUtility.GetObjectPickerObject();
                SetSmartValue(picker as Texture2D);
                _activeControlId = 0;
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
                    _activeControlId = GUIUtility.GetControlID (FocusType.Passive);
                    EditorGUIUtility.ShowObjectPicker<Texture2D> (memberVal, true, "", _activeControlId);
                }
            }
        }

        private bool CheckIfIsReadOnly()
        {
            bool isReadOnly = false;
            var info = HostInfo.MemberInfo;
            if (info is PropertyInfo propertyInfo)
                isReadOnly = propertyInfo.GetSetMethod(false) == null;

            return isReadOnly || !GUI.enabled;
        }
    }
}