using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class InlineEditorWrapper : BaseWrapperDrawable
    {
        private readonly IDrawableRead _readableDrawable;
        private SerializedObject _serializedObj;
        private IOrderedDrawable _inlineDrawable;

        public InlineEditorWrapper(IDrawableRead drawable) 
            : base(drawable)
        {
            _readableDrawable = drawable;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            CreateBackingObjects();
        }

        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            if (!ReferenceEquals(_serializedObj.targetObject, _readableDrawable.GetValue()))
                CreateBackingObjects();
        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            if (_inlineDrawable != null)
                _inlineDrawable.Draw(GUIContent.none, options);
            else
                base.DrawInner(label, options);
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (_inlineDrawable != null)
                _inlineDrawable.Draw(rect, GUIContent.none);
            else
                base.DrawInner(rect, label);
        }

        private void CreateBackingObjects()
        {
            _serializedObj = new SerializedObject(_readableDrawable.GetValue() as UnityEngine.Object);
            _inlineDrawable = DrawableFactory.CreateDrawableFor(_serializedObj);
        }

        [WrapDrawer(typeof(InlineEditorAttribute), -10)]
        public static BaseWrapperDrawable Create(InlineEditorAttribute attr, IOrderedDrawable drawable)
        {
            if (!drawable.HostInfo.GetReturnType().InheritsFrom(typeof(UnityEngine.Object)))
                return null;
            if (drawable is IDrawableRead readableDrawable)
                return new InlineEditorWrapper(readableDrawable);
            return null;
        }
    }
    
}