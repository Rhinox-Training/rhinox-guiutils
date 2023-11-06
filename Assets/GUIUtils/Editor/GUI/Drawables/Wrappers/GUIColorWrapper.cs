using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class GUIColorWrapper : BaseWrapperDrawable
    {
        public Color _color;
        private Color _previousState;

        public GUIColorWrapper(IOrderedDrawable drawable) : base(drawable)
        {
            
        }

        protected override void OnPreDraw()
        {
            _previousState = GUI.backgroundColor;
            GUI.backgroundColor = _color;
            
            base.OnPreDraw();
        }

        protected override void OnPostDraw()
        {
            GUI.backgroundColor = _previousState;
            base.OnPostDraw();
        }

        [WrapDrawer(typeof(GUIColorAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(GUIColorAttribute attr, IOrderedDrawable drawable)
        {
            return new GUIColorWrapper(drawable)
            {
                _color = attr.Color
            };
        }
    }
}