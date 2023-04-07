using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DisplayAsStringWrapper : BaseWrapperDrawable
    {
        public override float ElementHeight
        {
            get
            {
                if (_memberDrawable != null || _objectDrawable != null)
                    return EditorGUIUtility.singleLineHeight;
                return base.ElementHeight;
            }
        }

        private readonly IMemberDrawable _memberDrawable;
        private readonly IObjectDrawable _objectDrawable;

        public DisplayAsStringWrapper(IOrderedDrawable drawable) : base(drawable)
        {
            if (drawable is IMemberDrawable memberDrawable)
                _memberDrawable = memberDrawable;
            if (drawable is IObjectDrawable objectDrawable)
                _objectDrawable = objectDrawable;
        } 
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            if (GetDisplayText(out GUIContent overrideText))
            {
                EditorGUILayout.LabelField(overrideText, options);
                return;
            }

            base.DrawInner(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (GetDisplayText(out GUIContent overrideText))
            {
                EditorGUI.LabelField(rect, overrideText);
                return;
            }

            base.DrawInner(rect, label);
        }

        private bool GetDisplayText(out GUIContent contentLabel)
        {
            if (_memberDrawable != null)
            {
                object value = _memberDrawable.Entry.GetValue();
                contentLabel = value != null ? GUIContentHelper.TempContent(value.ToString()) : GUIContent.none;
                return true;
            }

            if (_objectDrawable != null)
            {
                contentLabel = _objectDrawable.Instance != null ? GUIContentHelper.TempContent(_objectDrawable.Instance.ToString()) : GUIContent.none;
                return true;
            }

            contentLabel = null;
            return false;
        }

        [WrapDrawer(typeof(DisplayAsStringAttribute), -10)]
        public static BaseWrapperDrawable Create(DisplayAsStringAttribute attr, IOrderedDrawable drawable)
        {
            return new DisplayAsStringWrapper(drawable)
            {
            };
        }
    }
}