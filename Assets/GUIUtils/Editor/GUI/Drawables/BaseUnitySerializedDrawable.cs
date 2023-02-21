using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseUnitySerializedDrawable : IOrderedDrawable
    {
        public Color? Colour { get; set; }
        public float Order { get; set; }

        public virtual float ElementHeight => 18.0f;

        private readonly SerializedObject _serializedObj;

        protected BaseUnitySerializedDrawable(SerializedObject obj, float order = 0)
        {
            //if (obj == null) throw new ArgumentNullException(nameof(obj));
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

        public virtual ICollection<TAttribute> GetMemberAttributes<TAttribute>() where TAttribute : Attribute
        {
            return Array.Empty<TAttribute>();
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
            if (_serializedObj == null || _serializedObj.targetObjects == null)
                return;
            
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