using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class IndentWrapper : BaseWrapperDrawable
    {
        private int _indent;
        
        public IndentWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override void OnPreDraw()
        {
            base.OnPreDraw();
            EditorGUI.indentLevel += _indent;
        }

        protected override void OnPostDraw()
        {
            EditorGUI.indentLevel -= _indent;
            base.OnPostDraw();
        }

        [WrapDrawer(typeof(IndentAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(IndentAttribute attr, IOrderedDrawable drawable)
        {
            return new IndentWrapper(drawable)
            {
                _indent = attr.IndentLevel,
            };
        }
    }
}