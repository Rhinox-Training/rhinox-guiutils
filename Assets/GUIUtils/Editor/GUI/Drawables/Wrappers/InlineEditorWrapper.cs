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

        private bool _expanded;
        private InlineEditorObjectFieldModes _style;
        
        public override float ElementHeight => GetElementHeight();

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
            GUI.Scope scope = null;
            bool foldout = false;
            switch (_style)
            {
                case InlineEditorObjectFieldModes.Foldout:
                    scope = new eUtility.VerticalGroup();
                    foldout = true;
                    break;
                case InlineEditorObjectFieldModes.Boxed:
                    scope = new eUtility.Box();
                    foldout = true;
                    break;
                case InlineEditorObjectFieldModes.Hidden:
                    scope = new eUtility.HiddenGroup(_serializedObj?.targetObject != null);
                    break;
                case InlineEditorObjectFieldModes.CompletelyHidden:
                    scope = new eUtility.HiddenGroup();
                    break;
            }
            
            _expanded = eUtility.FoldoutHeader(_expanded, label, out Rect contentRect, boxStyle: CustomGUIStyles.ToggleGroupHeader);
            using (new eUtility.VerticalGroup(CustomGUIStyles.ToggleGroupContent))
                base.DrawInner(contentRect, GUIContent.none);

            if (_expanded || !foldout)
            {
                if (_inlineDrawable != null)
                    _inlineDrawable.Draw(GUIContent.none, options);
                
                GUILayout.Space(CustomGUIUtility.Padding);
            }
            
            if (scope != null)
                scope.Dispose();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            base.DrawInner(rect, label);

            if (_expanded && _inlineDrawable != null)
                _inlineDrawable.Draw(rect, GUIContent.none);
        }

        private float GetElementHeight()
        {
            var height = base.ElementHeight;
            
            switch (_style)
            {
                case InlineEditorObjectFieldModes.Hidden:
                    if (_serializedObj?.targetObject != null)
                        height = 0;
                    break;
                case InlineEditorObjectFieldModes.CompletelyHidden:
                    height = 0;
                    break;
            }

            if (_expanded && _inlineDrawable != null)
                height += _inlineDrawable.ElementHeight;
            
            return height;
        }
        
        private void CreateBackingObjects()
        {
            _serializedObj = new SerializedObject(_readableDrawable.GetValue() as UnityEngine.Object);
            _inlineDrawable = DrawableFactory.CreateDrawableFor(_serializedObj);
        }

        [WrapDrawer(typeof(InlineEditorAttribute), Priority.Important)]
        public static BaseWrapperDrawable Create(InlineEditorAttribute attr, IOrderedDrawable drawable)
        {
            if (!drawable.HostInfo.GetReturnType().InheritsFrom(typeof(UnityEngine.Object)))
                return null;
            if (drawable is IDrawableRead readableDrawable)
                return new InlineEditorWrapper(readableDrawable)
                {
                    _expanded = attr.Expanded,
                    _style = attr.ObjectFieldMode
                };
            return null;
        }
    }
    
}