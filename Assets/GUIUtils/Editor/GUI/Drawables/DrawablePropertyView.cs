using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawablePropertyView
    {
        private readonly object _instance;
        private readonly SerializedObject _serializedObject;
        private readonly ICollection<IOrderedDrawable> _drawables;

        public float Height
        {
            get
            {
                float height = 0.0f;
                foreach (var drawable in _drawables)
                    height += drawable.ElementHeight;
                return height + (Math.Max(0, _drawables.Count - 1) * 2.0f);
            }
        }

        public DrawablePropertyView(object unityObjInstance, bool forceDrawAsUnityObject = false)
        {
            if (unityObjInstance == null) throw new ArgumentNullException(nameof(unityObjInstance));
            _instance = unityObjInstance;
            _serializedObject = null;
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject(unityObjInstance)};
            else
                _drawables = ParseNonUnityObject(unityObjInstance);
        }
        
        public DrawablePropertyView(SerializedObject serializedObject, bool forceDrawAsUnityObject = false)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            _instance = serializedObject;
            _serializedObject = serializedObject;
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject(serializedObject.targetObject)};
            else
                _drawables = ParseSerializedObject(serializedObject);
        }
        
        public DrawablePropertyView(SerializedProperty property, bool forceDrawAsUnityObject = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _instance = property;
            _serializedObject = property.serializedObject;
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject(property.GetValue())};
            else
                _drawables = ParseSerializedProperty(property);
        }
        
        public static ICollection<IOrderedDrawable> ParseNonUnityObject(object obj)
        {
            if (obj == null)
                return Array.Empty<BaseEntityDrawable>();

            var type = obj.GetType();

            var drawables = DrawableFactory.CreateDrawableMembersFor(obj, type);

            if (drawables.Count == 0 && obj is UnityEngine.Object unityObj)
                drawables.Add(new DrawableUnityObject(unityObj, null));

            if (!drawables.IsNullOrEmpty())
            {
                DrawableGroupingHelper.Process(ref drawables);

                drawables.SortDrawables();
            }

            return drawables;
        }

        public static ICollection<IOrderedDrawable> ParseSerializedProperty(SerializedProperty property)
        {
            if (property == null)
                return Array.Empty<IOrderedDrawable>();

            var hostInfo = property.GetHostInfo();
            var type = hostInfo.GetReturnType();

            if (AttributeParser.ParseDrawAsUnity(hostInfo.FieldInfo))
                return new[] {new DrawableUnityObject(property.GetValue())};

            var drawables = DrawableFactory.CreateDrawableMembersFor(property, type);

            if (!drawables.IsNullOrEmpty())
            {
                DrawableGroupingHelper.Process(ref drawables);

                drawables.SortDrawables();
            }

            return drawables;
        }

        public static ICollection<IOrderedDrawable> ParseSerializedObject(SerializedObject obj)
        {
            if (obj == null || obj.targetObject == null)
                return Array.Empty<IOrderedDrawable>();

            var type = obj.targetObject.GetType();

            var drawables = DrawableFactory.CreateDrawableMembersFor(obj, type);

            if (!drawables.IsNullOrEmpty())
            {
                DrawableGroupingHelper.Process(ref drawables);

                drawables.SortDrawables();
            }
            return drawables;
        }
        
        public void DrawLayout()
        {
            if (_drawables == null)
                return;

            OnPreDraw();
            
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                drawable.Draw();
            }

            OnPostDraw();
        }

        public void Draw(Rect rect)
        {
            if (_drawables == null)
                return;
            
            OnPreDraw();
            
            foreach (var drawable in _drawables)
            {
                if (drawable == null)
                    continue;
                rect.height = drawable.ElementHeight;
                drawable.Draw(rect);
                rect.y += rect.height + 2.0f;
            }
            
            OnPostDraw();
        }

        private void OnPreDraw()
        {
            if (_serializedObject != null)
            {
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }
        }

        private void OnPostDraw()
        {
            if (_serializedObject != null)
                _serializedObject.ApplyModifiedProperties();
        }
    }
}