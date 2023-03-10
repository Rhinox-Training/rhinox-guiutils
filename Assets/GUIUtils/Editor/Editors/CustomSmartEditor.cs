using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class CustomSmartEditor : UnityEditor.Editor
    {
        private DrawablePropertyView _propertyView;

        public override void OnInspectorGUI()
        {
            var attr = target.GetType().GetCustomAttribute<SmartFallbackDrawnAttribute>();
            if (attr == null)
            {
                base.OnInspectorGUI();
                return;
            }

            if (attr.AllowUnityIfAble)
            {
                int count = CountDrawnProperties(serializedObject);
                if (count > 0)
                {
                    base.OnInspectorGUI();
                    return;
                }
            }

            if (_propertyView == null)
                _propertyView = new DrawablePropertyView(serializedObject);
            
            _propertyView.DrawLayout();
        }
        
        private static int CountDrawnProperties(SerializedObject obj)
        {
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            int count = 0;
            while (iterator.NextVisible(enterChildren))
            {
                count++;
                enterChildren = false;
            }

            return count;
        }
    }
}