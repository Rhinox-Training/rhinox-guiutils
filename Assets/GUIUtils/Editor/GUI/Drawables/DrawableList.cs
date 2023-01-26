using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class DrawableList : SimpleDrawable
    {
        private BetterReorderableList _listRO;
        private readonly SerializedProperty _listProperty;
        private readonly ListDrawerSettingsAttribute _listDrawerAttr;
        
        private ListElementDrawable[] _listElements;

        private class ListElementDrawable
        {
            private readonly ICollection<ISimpleDrawable> _drawables;
            private readonly SerializedProperty _property;

            public ListElementDrawable(SerializedProperty property)
            {
                if (property == null) throw new ArgumentNullException(nameof(property));
                _property = property;
                if (property.exposedReferenceValue != null)
                    _drawables = DrawableFactory.ParseSerializedObject(new SerializedObject(property.exposedReferenceValue));
            }

            public void Draw(Rect r)
            {
                if (_drawables == null)
                {
                    EditorGUI.PropertyField(r, _property, true);
                    return;
                }

                foreach (var drawable in _drawables)
                    drawable.Draw(r);
            }
        }
        
        public DrawableList(SerializedProperty listProperty)
            : base(listProperty.serializedObject)
        {
            if (listProperty == null) throw new ArgumentNullException(nameof(listProperty));
            _listProperty = listProperty;

            _listDrawerAttr = listProperty.GetAttributeOrCreate<ListDrawerSettingsAttribute>(); // TODO: handle defaults

            _listRO = new PageableReorderableList(listProperty.serializedObject, listProperty,
                _listDrawerAttr.DraggableItems, true, 
                !_listDrawerAttr.IsReadOnly && !_listDrawerAttr.HideAddButton, !_listDrawerAttr.IsReadOnly && !_listDrawerAttr.HideRemoveButton)
            {
                MaxItemsPerPage = _listDrawerAttr.NumberOfItemsPerPage
            };
            
            _listElements = new ListElementDrawable[_listRO.count];
            
            _listRO.showDefaultBackground = false;
            _listRO.drawElementCallback = DrawElement;
            _listRO.onChangedCallback += OnChangedListCallback;
        }
        
        protected override void Draw(UnityEngine.Object target)
        {
            if (_listProperty != null && _listProperty.serializedObject != null)
            {
                _listProperty.serializedObject.Update();
                EditorGUI.BeginDisabledGroup(_listDrawerAttr.IsReadOnly);
                {
                    _listRO.DoLayoutList();
                }
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
                {
                    _listRO.DoList(rect);
                }
                EditorGUI.EndDisabledGroup();
                _listProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnChangedListCallback(BetterReorderableList list)
        {
            _listElements = new ListElementDrawable[list.count];
        }
        
        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var listEntryRect = _listDrawerAttr.IsReadOnly ? rect : rect.AlignLeft(rect.width - 16);
            
            if (_listElements[index] == null)
                _listElements[index] = new ListElementDrawable(_listRO.serializedProperty.GetArrayElementAtIndex(index));
            _listElements[index].Draw(listEntryRect);
            
            //EditorGUI.PropertyField(listEntryRect, _listRO.serializedProperty.GetArrayElementAtIndex(index), GUIContent.none);
        }
    }
}