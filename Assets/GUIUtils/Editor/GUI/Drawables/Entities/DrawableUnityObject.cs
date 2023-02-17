using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableUnityObject : SimpleDrawable
    {
        public DrawableUnityObject(object obj, int order = 0) : base(obj, order)
        {
        }

        protected override void Draw(object target)
        {
            EditorGUILayout.ObjectField(GUIContentHelper.TempContent(target.GetType().Name), target as UnityEngine.Object, target.GetType(), true);
        }

        protected override void Draw(Rect rect, object target)
        {
            EditorGUI.ObjectField(rect, GUIContentHelper.TempContent(target.GetType().Name), target as UnityEngine.Object, target.GetType(), true);
        }
    }
}