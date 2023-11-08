using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    public class CustomSmartEditor : BaseEditor<Object>
    {
        private DrawablePropertyView _propertyView;
        
        public override void OnInspectorGUI()
        {
            var attr = AttributeProcessorHelper.FindAttributeInclusive<SmartFallbackDrawnAttribute>(target.GetType());
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
            {
                _propertyView = new DrawablePropertyView(serializedObject);
                _propertyView.RepaintRequested += RequestRepaint;
            }
            
            DrawScriptField();
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