using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableList : SimpleDrawable
    {
        private BetterReorderableList _listRO;
        private readonly SerializedProperty _listProperty;
        private readonly ListDrawerSettingsAttribute _listDrawerAttr;
        private readonly int _maxPerPage;
        private readonly float _elementHeight;

        private class ElementDrawable
        {
            private readonly ICollection<ISimpleDrawable> _drawables;

            public ElementDrawable(SerializedProperty property)
            {
                if (property.exposedReferenceValue != null)
                    _drawables = DrawableFactory.ParseSerializedObject(new SerializedObject(property.exposedReferenceValue));
            }

            public void Draw(Rect r)
            {
                if (_drawables == null)
                {
                    GUI.TextField(r, "NULL");
                    return;
                }

                foreach (var drawable in _drawables)
                    drawable.Draw(r);
            }
        }
        

        private static BetterReorderableList.Defaults _defaults;
        private ElementDrawable[] _listElements;

        private static BetterReorderableList.Defaults Defaults
        {
            get
            {
                if (_defaults == null)
                {
                    var field = typeof(BetterReorderableList).GetField("s_Defaults", BindingFlags.Static | BindingFlags.NonPublic);
                    _defaults = field.GetValue(null) as BetterReorderableList.Defaults;
                }
                return _defaults;
            }
        }

        public DrawableList(SerializedProperty listProperty)
            : base(listProperty.serializedObject)
        {
            if (listProperty == null) throw new ArgumentNullException(nameof(listProperty));
            _listProperty = listProperty;

            _listDrawerAttr = GetAttribute<ListDrawerSettingsAttribute>(listProperty) ??
                              new ListDrawerSettingsAttribute(); // TODO: handle defaults



            _listRO = new BetterReorderableList(listProperty.serializedObject, listProperty, _listDrawerAttr.DraggableItems,
                true, !_listDrawerAttr.HideAddButton, !_listDrawerAttr.HideRemoveButton);
            _listElements = new ElementDrawable[_listRO.count];

            _maxPerPage = _listDrawerAttr.NumberOfItemsPerPage;
            _elementHeight = _listRO.elementHeight;

            _listRO.drawFooterCallback = DrawFooter;

            _listRO.drawHeaderCallback = rect =>
            {
                var nameRect = rect.AlignLeft(rect.width);
                var sizeRect = rect.AlignRight(rect.width / 4.0f);
                EditorGUI.LabelField(nameRect, listProperty.displayName);
                EditorGUI.LabelField(sizeRect, $"{_listRO.count} Items");
            };
            _listRO.showDefaultBackground = false;
            //_listRO.elementHeightCallback = ElementHeight;
            _listRO.drawElementBackgroundCallback = DrawElementBackground;
            _listRO.drawElementCountCallback = GetDrawnElementCount;
            _listRO.drawElementCallback = (rect, index, active, focused) =>
            {
                if (_maxPerPage > 0 && index > _maxPerPage)
                    return;
                rect.y += 2.0f;
                rect.height = EditorGUIUtility.singleLineHeight;

                DrawElement(rect, index);

                if (!_listDrawerAttr.IsReadOnly)
                {
                    if (GUI.Button(rect.AlignRight(16), GUIContentHelper.TempContent("X", "Remove entry")))
                        DeleteEntry(index);
                }
            };
        }

        private void DeleteEntry(int index)
        {
            var list = GetValue(_listRO.serializedProperty) as IList;
            try
            {
                if (list != null)
                {
                    list.RemoveAt(index);
                    _listElements = new ElementDrawable[_listRO.count];
                    SetValue(_listRO.serializedProperty, list);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private int GetDrawnElementCount()
        {
            if (_maxPerPage > 0)
                return Mathf.Min(_maxPerPage, _listRO.count);
            return _listRO.count;
        }

        private void DrawElement(Rect rect, int index)
        {
            var listEntryRect = _listDrawerAttr.IsReadOnly ? rect : rect.AlignLeft(rect.width - 16);
            
            if (_listElements[index] == null)
                _listElements[index] = new ElementDrawable(_listRO.serializedProperty.GetArrayElementAtIndex(index));
            _listElements[index].Draw(listEntryRect);
            
            //EditorGUI.PropertyField(listEntryRect, _listRO.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none);
        }

        private void DrawFooter(Rect rect)
        {
            if (_listDrawerAttr.HideAddButton && _listDrawerAttr.HideRemoveButton)
                return;

            if (_maxPerPage > 0)
            {
                var list = GetValue(_listRO.serializedProperty) as IList;
                if (list != null && list.Count > _maxPerPage)
                    rect.y -= (list.Count - _maxPerPage - 1) * _elementHeight;
            }
            
            Defaults.DrawFooter(rect, _listRO);
        }

        private float ElementHeight(int index)
        {
            if (_maxPerPage > 0 && index > _maxPerPage)
                return 0.0f;
            return _elementHeight;
        }

        public static object GetValue(SerializedProperty property)
        {
            System.Type parentType = property.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);
            return fi.GetValue(property.serializedObject.targetObject);
        }

        public static void SetValue(SerializedProperty property, object value)
        {
            System.Type parentType = property.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo
                fi = parentType.GetField(property.propertyPath); //this FieldInfo contains the type.
            fi.SetValue(property.serializedObject.targetObject, value);
        }

        public static T GetAttribute<T>(SerializedProperty property) where T : Attribute
        {
            System.Type parentType = property.serializedObject.targetObject.GetType();
            System.Reflection.FieldInfo fi = parentType.GetField(property.propertyPath);
            return fi.GetCustomAttribute(typeof(T)) as T;
        }

        protected override void Draw(UnityEngine.Object target)
        {
            //base.OnInspectorGUI();
            if (_listProperty != null && _listProperty.serializedObject != null)
            {
                _listProperty.serializedObject.Update();
                EditorGUI.BeginDisabledGroup(_listDrawerAttr.IsReadOnly);
                _listRO.DoLayoutList();
                
                
                
                EditorGUI.EndDisabledGroup();
                _listProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        protected override void Draw(Rect rect, Object target)
        {
            if (_listProperty != null && _listProperty.serializedObject != null)
            {
                _listProperty.serializedObject.Update();
                EditorGUI.BeginDisabledGroup(_listDrawerAttr.IsReadOnly);
                _listRO.DoList(rect);
                
                
                
                EditorGUI.EndDisabledGroup();
                _listProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        public void DrawElementBackground(
            Rect rect,
            int index,
            bool selected,
            bool focused)
        {
            if (Event.current.type != UnityEngine.EventType.Repaint)
                return;
            if (_maxPerPage > 0 && index > _maxPerPage)
                return;
            Defaults.elementBackground.Draw(rect, false, selected, selected, focused);
        }
    }
}