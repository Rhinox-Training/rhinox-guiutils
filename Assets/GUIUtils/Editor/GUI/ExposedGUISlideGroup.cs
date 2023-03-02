using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ExposedGUISlideGroup
    {
        internal static ExposedGUISlideGroup current = (ExposedGUISlideGroup) null;
        private Dictionary<int, Rect> animIDs = new Dictionary<int, Rect>();
        private const float kLerp = 0.1f;
        private const float kSnap = 0.5f;

        public void Begin()
        {
            if (ExposedGUISlideGroup.current != null)
                Debug.LogError((object) "You cannot nest animGroups");
            else
                ExposedGUISlideGroup.current = this;
        }

        public void End() => ExposedGUISlideGroup.current = (ExposedGUISlideGroup) null;

        public void Reset()
        {
            ExposedGUISlideGroup.current = (ExposedGUISlideGroup) null;
            this.animIDs.Clear();
        }

        public Rect GetRect(int id, Rect r) =>
            Event.current.type != UnityEngine.EventType.Repaint ? r : this.GetRect(id, r, out bool _);

        private Rect GetRect(int id, Rect r, out bool changed)
        {
            if (!this.animIDs.ContainsKey(id))
            {
                this.animIDs.Add(id, r);
                changed = false;
                return r;
            }

            Rect animId = this.animIDs[id];
            if ((double) animId.y != (double) r.y || (double) animId.height != (double) r.height ||
                (double) animId.x != (double) r.x || (double) animId.width != (double) r.width)
            {
                float t = 0.1f;
                if ((double) Mathf.Abs(animId.y - r.y) > 0.5)
                    r.y = Mathf.Lerp(animId.y, r.y, t);
                if ((double) Mathf.Abs(animId.height - r.height) > 0.5)
                    r.height = Mathf.Lerp(animId.height, r.height, t);
                if ((double) Mathf.Abs(animId.x - r.x) > 0.5)
                    r.x = Mathf.Lerp(animId.x, r.x, t);
                if ((double) Mathf.Abs(animId.width - r.width) > 0.5)
                    r.width = Mathf.Lerp(animId.width, r.width, t);
                this.animIDs[id] = r;
                changed = true;
                HandleUtility.Repaint();
            }
            else
                changed = false;

            return r;
        }
    }
}