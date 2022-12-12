using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class SimpleDrawable : ISimpleDrawable
    {
        public Color? Colour { get; set; }
        public int Order { get; set; }

        private readonly SerializedObject _serializedObj;

        protected SimpleDrawable(SerializedObject obj, int order = 0)
        {
            _serializedObj = obj;
            Order = order;
        }

        public void Draw()
        {
            if (Colour != null)
            {
                var color = Colour.Value;
                using (new eUtility.GuiBackgroundColor(color))
                {
                    DrawForTargets();
                }
            }
            else
            {
                DrawForTargets();
            }
        }

        private void DrawForTargets()
        {
            foreach (var target in _serializedObj.targetObjects)
            {
                if (target == null)
                    continue;
                Draw(target);
            }
        }

        protected abstract void Draw(Object target);
    }
}