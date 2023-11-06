using Rhinox.GUIUtils.Attributes;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class ForceWideModeWrapper : BaseWrapperDrawable
    {
        private bool _previousState;

        public override float ElementHeight
        {
            get
            {
                bool wasWideMode = EditorGUIUtility.wideMode;
                EditorGUIUtility.wideMode = true;
                float elementHeight = base.ElementHeight;
                EditorGUIUtility.wideMode = wasWideMode;
                return elementHeight;
            }
        }

        public ForceWideModeWrapper(IOrderedDrawable drawable) 
            : base(drawable) { }
        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            _previousState = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
        }

        protected override void OnPostDraw()
        {
            EditorGUIUtility.wideMode = _previousState;
            base.OnPostDraw();
        }
        
        [WrapDrawer(typeof(ForceWideModeAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(ForceWideModeAttribute attr, IOrderedDrawable drawable)
        {
            return new ForceWideModeWrapper(drawable);
        }
    }
}