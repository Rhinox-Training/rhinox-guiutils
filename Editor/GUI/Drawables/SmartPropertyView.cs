using System;
using UnityEditor;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Rhinox.GUIUtils.Editor
{
    public class SmartPropertyView
    {
#if ODIN_INSPECTOR
        private readonly PropertyTree _propertyTree;
        private readonly bool _allowUndo;
#else
        private readonly DrawablePropertyView _odinlessDrawer;
#endif

        public event Action RepaintRequested;
        
        public SmartPropertyView(object instance)
        {
#if ODIN_INSPECTOR
            _propertyTree = PropertyTree.Create(instance);
            _allowUndo = instance is UnityEngine.Object;
#else
            _odinlessDrawer = new DrawablePropertyView(instance);
            _odinlessDrawer.RepaintRequested += RequestRepaint;
#endif
        }
        public SmartPropertyView(SerializedObject serializedObject)
        {
#if ODIN_INSPECTOR
            _propertyTree = PropertyTree.Create(serializedObject);
            _allowUndo = true;
#else
            _odinlessDrawer = new DrawablePropertyView(serializedObject);
            _odinlessDrawer.RepaintRequested += RequestRepaint;
#endif
        }

        public void DrawLayout()
        {
#if ODIN_INSPECTOR
            _propertyTree.Draw(_allowUndo);
#else
            _odinlessDrawer.DrawLayout();
#endif
        }

        private void RequestRepaint()
        {
            RepaintRequested?.Invoke();
        }
    }
}