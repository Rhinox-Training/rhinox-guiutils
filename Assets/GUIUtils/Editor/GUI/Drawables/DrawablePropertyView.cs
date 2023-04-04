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
        private readonly GenericMemberEntry _entry;
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

        public DrawablePropertyView(object instance, bool forceDrawAsUnityObject = false)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _instance = instance;
            _entry = null;
            _serializedObject = null;
            
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject((UnityEngine.Object) instance)};
            else
                _drawables = ParseNonUnityObject(instance);
        }
        
        public DrawablePropertyView(GenericMemberEntry entry, bool forceDrawAsUnityObject = false)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            _entry = entry;
            _instance = _entry.Instance;
            _serializedObject = null;
            
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject((UnityEngine.Object) entry.GetValue(), entry.Info)};
            else
                _drawables = DrawableFactory.CreateDrawableMembersFor(entry);
        }
        
        public DrawablePropertyView(SerializedObject serializedObject, bool forceDrawAsUnityObject = false)
        {
            if (serializedObject == null) throw new ArgumentNullException(nameof(serializedObject));
            _instance = serializedObject;
            _serializedObject = serializedObject;
            _entry = null;
            
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
            _entry = null;
            
            if (forceDrawAsUnityObject)
                _drawables = new[] {new DrawableUnityObject((UnityEngine.Object) property.GetValue())};
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

            if (drawables.IsNullOrEmpty())
                return drawables;
            
            var group = GroupingHelper.Process(drawables);
            group.Sort();
            return new IOrderedDrawable[] { group };
        }

        public static ICollection<IOrderedDrawable> ParseSerializedProperty(SerializedProperty property)
        {
            if (property == null)
                return Array.Empty<IOrderedDrawable>();

            var hostInfo = property.GetHostInfo();
            var type = hostInfo.GetReturnType();

            if (AttributeParser.ParseDrawAsUnity(hostInfo.FieldInfo))
                return new[] {new DrawableUnityObject((UnityEngine.Object)property.GetValue(), property.FindFieldInfo())};

            var drawables = DrawableFactory.CreateDrawableMembersFor(property, type);

            if (drawables.IsNullOrEmpty()) return drawables;
            
            var group = GroupingHelper.Process(drawables);
            group.Sort();
            return new IOrderedDrawable[] { group };
        }

        public static ICollection<IOrderedDrawable> ParseSerializedObject(SerializedObject obj)
        {
            if (obj == null || obj.targetObject == null)
                return Array.Empty<IOrderedDrawable>();

            var type = obj.targetObject.GetType();

            var drawables = DrawableFactory.CreateDrawableMembersFor(obj, type);

            if (drawables.IsNullOrEmpty()) return drawables;
            
            var group = GroupingHelper.Process(drawables);
            group.Sort();
            return new IOrderedDrawable[] { group };
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
                drawable.Draw(drawable.Label);
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
                drawable.Draw(rect, drawable.Label);
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