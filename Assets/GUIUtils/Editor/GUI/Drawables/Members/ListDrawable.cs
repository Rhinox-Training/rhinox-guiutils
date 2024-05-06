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
    public class ListElementDrawable : IRepaintable
    {
        public float ElementHeight => _propertyView?.Height ?? _defaultElementHeight;

        private readonly float _defaultElementHeight;
        private readonly SerializedProperty _property;
        private readonly GenericHostInfo _entry;
        private readonly DrawablePropertyView _propertyView;

        public event Action RepaintRequested;

        public ListElementDrawable(SerializedProperty property, float defaultElementHeight = 18.0f)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            _defaultElementHeight = defaultElementHeight;
            _property = property;
            _propertyView = new DrawablePropertyView(property);
            _propertyView.RepaintRequested += RequestRepaint;
        }
        
        public ListElementDrawable(GenericHostInfo entry, float defaultElementHeight = 18.0f)
        {
            _entry = entry;
            _defaultElementHeight = defaultElementHeight;
            _property = null;
            _propertyView = new DrawablePropertyView(_entry);
            _propertyView.RepaintRequested += RequestRepaint;
        }

        public void Draw(Rect r)
        {
            if (r.IsValid())
                r.y += CustomGUIUtility.Padding;
            if (_propertyView != null)
                _propertyView.Draw(r);
        }

        public void RequestRepaint()
        {
            RepaintRequested?.Invoke();
        }
    }

    public class ListDrawable : BaseMemberDrawable
    {
        private readonly PageableReorderableList _listRO;
        private ListElementDrawable[] _listElements;

        private readonly SerializedProperty _listProperty;
        private bool _isReadOnly;

        public override float ElementHeight => _listRO.GetHeight();

        public ListDrawable(SerializedProperty listProperty)
            : base(listProperty.GetHostInfo())
        {
            if (listProperty == null)
                throw new ArgumentNullException(nameof(listProperty));
            
            _listProperty = listProperty;
            _listRO = new PageableReorderableList(listProperty, false);
            
            var listDisplaySettings = ListDisplaySettings.Create(_hostInfo);
            ApplyDrawerSettings(listDisplaySettings);

            Initialize(_listRO);
        }

        public ListDrawable(GenericHostInfo hostInfo) : base(hostInfo)
        {
            _listProperty = null;
            _listRO = new PageableReorderableList(hostInfo, false);
            
            var listDisplaySettings = ListDisplaySettings.Create(hostInfo);
            ApplyDrawerSettings(listDisplaySettings);
            
            Initialize(_listRO);
        }

        private void ApplyDrawerSettings(ListDisplaySettings settings)
        {
            if (settings.MaxItemsPerPage > 0)
                _listRO.MaxItemsPerPage = settings.MaxItemsPerPage;
            _listRO.Draggable = settings.DraggableItems;
            _listRO.DisplayAdd = !settings.IsReadOnly && !settings.HideAddButton;
            _listRO.DisplayRemove = !settings.IsReadOnly && !settings.HideRemoveButton;
            _listRO.DisplayHeader = !settings.HideHeader;
            _isReadOnly = settings.IsReadOnly;
            _listRO.SetExpanded(settings.Expanded);
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
            roList.RepaintRequested += RequestRepaint;
        }

        private float OnHeight(int index)
        {
            if (_listElements.Length > index && index >= 0 && _listElements[index] != null)
            {
                return _listElements[index].ElementHeight;
            }

            return _listRO.elementHeight;
        }
        
        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            OnBeginDraw();
            EditorGUI.BeginDisabledGroup(_isReadOnly);
            {
                _listRO.DoLayoutList(label);
            }
            EditorGUI.EndDisabledGroup();
            OnEndDraw();
        }
        
        protected override void DrawInner(Rect rect, GUIContent label)
        {
            OnBeginDraw();
            EditorGUI.BeginDisabledGroup(_isReadOnly);
            {
                _listRO.DoList(rect, label);
            }
            EditorGUI.EndDisabledGroup();
            OnEndDraw();
        }

        private void OnChangedListCallback(BetterReorderableList list)
        {
            // TODO should be able to improve this
            _listElements = new ListElementDrawable[_listRO.count];
            for (int i = 0; i < _listRO.count; ++i)
                _listElements[i] = CreateElementFor(i);
            RequestRepaint();
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (_listElements.Length != _listRO.count)
                _listElements = new ListElementDrawable[_listRO.count];

            if (!_listElements.HasIndex(index))
                return;
            
            if (_listElements[index] == null)
                _listElements[index] = CreateElementFor(index);
            _listElements[index].Draw(rect);
        }

        private ListElementDrawable CreateElementFor(int index)
        {
            ListElementDrawable drawable;
            if (_listProperty != null)
            {
                var element = _listProperty.GetArrayElementAtIndex(index);
                drawable = new ListElementDrawable(element, _listRO.elementHeight);
            }
            else 
                drawable = new ListElementDrawable(_hostInfo.CreateArrayElement(index), _listRO.elementHeight);

            drawable.RepaintRequested += RequestRepaint;
            return drawable;
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
        }
    }
}