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

        public void Draw(Rect rect)
        {
            if (Colour != null)
            {
                var color = Colour.Value;
                using (new eUtility.GuiBackgroundColor(color))
                {
                    DrawForTargets(rect);
                }
            }
            else
            {
                DrawForTargets(rect);
            }
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

        private void DrawForTargets(Rect? r = null)
        {
            foreach (var target in _serializedObj.targetObjects)
            {
                if (target == null)
                    continue;

                if (r.HasValue)
                    Draw(r.Value, target);
                else
                    Draw(target);
            }
        }

        protected abstract void Draw(Object target);
        protected abstract void Draw(Rect rect, Object target);
    }
}