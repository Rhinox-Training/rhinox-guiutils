using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class ListElementDrawable
    {
        public float ElementHeight => _propertyView?.Height ?? _defaultElementHeight;

        private readonly float _defaultElementHeight;
        private readonly SerializedProperty _property;
        private readonly DrawablePropertyView _propertyView;

        public ListElementDrawable(SerializedProperty property, float defaultElementHeight = 18.0f, bool drawElementsAsUnity = false)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _defaultElementHeight = defaultElementHeight;
            _property = property;
            _propertyView = new DrawablePropertyView(property, drawElementsAsUnity);
        }

        public ListElementDrawable(object element, float defaultElementHeight = 18.0f, bool drawElementsAsUnity = false)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _defaultElementHeight = defaultElementHeight;
            _property = null;
            _propertyView = new DrawablePropertyView(element, drawElementsAsUnity);
        }

        public void Draw(Rect r)
        {
            r.y += 2.0f;
            if (_propertyView != null)
                _propertyView.Draw(r);
        }
    }

    public class DrawableList : BaseEntityDrawable
    {
        private BetterReorderableList _listRO;
        private readonly ListDrawerSettingsAttribute _listDrawerAttr;
        private ListElementDrawable[] _listElements;

        private readonly SerializedProperty _listProperty;
        private readonly MemberInfo _listMemberInfo;
        private readonly object _listContainerInstance;
        private readonly bool _drawElementsAsUnity;

        public override float ElementHeight
        {
            get
            {
                if (_listRO != null)
                    return _listRO.GetHeight();
                return base.ElementHeight;
            }
        }

        public DrawableList(SerializedProperty listProperty)
            : base(listProperty.serializedObject)
        {
            if (listProperty == null)
                throw new ArgumentNullException(nameof(listProperty));
            
            _listProperty = listProperty;
            _listContainerInstance = null;
            _listMemberInfo = null;

            _listDrawerAttr = listProperty.GetAttributeOrCreate<ListDrawerSettingsAttribute>();
            _drawElementsAsUnity = listProperty.GetAttribute<DrawAsUnityObjectAttribute>() != null;

            _listRO = new PageableReorderableList(listProperty.serializedObject, listProperty,
                _listDrawerAttr.DraggableItems, true,
                !_listDrawerAttr.IsReadOnly && !_listDrawerAttr.HideAddButton,
                !_listDrawerAttr.IsReadOnly && !_listDrawerAttr.HideRemoveButton)
            {
                MaxItemsPerPage = _listDrawerAttr.NumberOfItemsPerPage
            };

            Initialize(_listRO);
        }

        public DrawableList(GenericMemberEntry entry) : base(entry)
        {
            _listProperty = null;
            _listContainerInstance = entry.Instance;
            _listMemberInfo = entry.Info;
            _listDrawerAttr = entry.GetAttribute<ListDrawerSettingsAttribute>() ?? new ListDrawerSettingsAttribute();
            _drawElementsAsUnity = entry.GetAttribute<DrawAsUnityObjectAttribute>() != null;

            _listRO = new PageableReorderableList(_listContainerInstance, _listMemberInfo,
                _listDrawerAttr.DraggableItems, true,
                !_listDrawerAttr.IsReadOnly && !_listDrawerAttr.HideAddButton,
                !_listDrawerAttr.IsReadOnly && !_listDrawerAttr.HideRemoveButton)
            {
                MaxItemsPerPage = _listDrawerAttr.NumberOfItemsPerPage
            };

            Initialize(_listRO);
        }

        private void Initialize(BetterReorderableList roList)
        {
            _listElements = new ListElementDrawable[roList.count];
            for (int i = 0; i < roList.count; ++i)
                _listElements[i] = CreateElementFor(i);

            //_listRO.showDefaultBackground = false;
            roList.drawElementCallback = DrawElement;
            roList.onChangedCallback += OnChangedListCallback;
            roList.elementHeightCallback = OnHeight;
        }

        private float OnHeight(int index)
        {
            if (_listElements.Length > index && index >= 0 && _listElements[index] != null)
            {
                return _listElements[index].ElementHeight;
            }

            return _listRO.elementHeight;
        }

        protected override void DrawInner(GUIContent label)
        {
            if (_listRO != null && _listDrawerAttr != null)
            {
                OnBeginDraw();
                EditorGUI.BeginDisabledGroup(_listDrawerAttr.IsReadOnly);
                {
                    _listRO.DoLayoutList();
                }
                EditorGUI.EndDisabledGroup();
                OnEndDraw();
            }
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (_listRO != null && _listDrawerAttr != null)
            {
                OnBeginDraw();
                EditorGUI.BeginDisabledGroup(_listDrawerAttr.IsReadOnly);
                {
                    _listRO.DoList(rect);
                }
                EditorGUI.EndDisabledGroup();
                OnEndDraw();
            }
        }

        private void OnChangedListCallback(BetterReorderableList list)
        {
            _listElements = new ListElementDrawable[list.count];
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.IsValid())
                rect = _listDrawerAttr.IsReadOnly ? rect : rect.AlignLeft(rect.width - 16);

            if (_listElements.Length != _listRO.count)
                _listElements = new ListElementDrawable[_listRO.count];

            if (!_listElements.HasIndex(index))
                return;
            
            if (_listElements[index] == null)
                _listElements[index] = CreateElementFor(index, _drawElementsAsUnity);
            _listElements[index].Draw(rect);
        }

        private ListElementDrawable CreateElementFor(int index, bool drawElementsAsUnity = false)
        {
            if (_listProperty != null)
            {
                var element = _listProperty.GetArrayElementAtIndex(index);
                return new ListElementDrawable(element, _listRO.elementHeight, drawElementsAsUnity);
            }
            else
            {
                var value = _listMemberInfo.GetValue(_listContainerInstance) as IList;
                var nonUnityElement = value[index];
                return new ListElementDrawable(nonUnityElement, _listRO.elementHeight, drawElementsAsUnity);
            }
        }

        private void OnBeginDraw()
        {
            if (_listProperty == null) return;
            
            if (_listProperty.serializedObject != null)
            {
                // NOTE: Apply anything modified so nothing gets cleared when drawing nested
                _listProperty.serializedObject.ApplyModifiedProperties(); 
                _listProperty.serializedObject.Update();
            }
        }

        private void OnEndDraw()
        {
            if (_listProperty != null)
            {
                if (_listProperty.serializedObject != null)
                    _listProperty.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                _listMemberInfo.SetValue(_listContainerInstance, _listRO.list);
            }
        }
    }
}