﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public class PageableReorderableList : BetterReorderableList
    {
        public int MaxItemsPerPage { get; set; } = DEFAULT_ITEMS_PER_PAGE;
        private const int DEFAULT_ITEMS_PER_PAGE = 100;
        
        private ICollection<Type> m_AddOptionTypes;
        
        private int _drawPageIndex;
        private int _maxPagesCount => Mathf.CeilToInt((float)List.Count / MaxItemsPerPage);

        private bool _isUnityType;
        
        public override object SelectedItem => List[SelectedIndex + MaxItemsPerPage * _drawPageIndex];


        // Each tracks their own rect so you do need multiple
        private readonly List<HoverTexture> _closeIcons = new List<HoverTexture>();

        private Rect _headerRect;
        private GUIContent _addContent;

        private static Dictionary<Type, TypeCache.TypeCollection> _typeOptionsByType = new Dictionary<Type, TypeCache.TypeCollection>();
        private readonly GenericHostInfo _hostInfo;
        
        private bool HasMultipleTypeOptions
        {
            get 
            {
                if (m_AddOptionTypes != null)
                {
                    if (m_AddOptionTypes.Count > 1)
                        return true;
                    if (m_AddOptionTypes.Count == 1)
                        return m_AddOptionTypes.First() != this.m_ElementType;
                }

                return false;
            }
        }
        
        public PageableReorderableList(IList elements, bool draggable = true, bool displayHeader = true, bool displayAdd = true, bool displayRemove = true)
            : base(elements, draggable, displayHeader, displayAdd, displayRemove)
        {
        }
        
        public PageableReorderableList(SerializedObject serializedObject, SerializedProperty elements, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true) 
            : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
        }

        public PageableReorderableList(GenericHostInfo hostInfo, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
            : base(hostInfo.GetSmartValue<IList>(), draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
            _hostInfo = hostInfo;
        }

        protected override void InitList(SerializedObject serializedObject, SerializedProperty elements, IList elementList, 
            bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
        {
            base.InitList(serializedObject, elements, elementList, draggable, displayHeader, displayAddButton, displayRemoveButton);

            _isUnityType = m_ElementType != null && m_ElementType.InheritsFrom<Object>();
            _addContent = new GUIContent(UnityIcon.AssetIcon("Fa_Plus"), tooltip: "Add Item");
            
            if (this.displayAdd && this.m_ElementType != null)
            {
                var options = new HashSet<Type>();
                if (!m_ElementType.IsAbstract)
                    options.Add(this.m_ElementType);
                if (!_typeOptionsByType.ContainsKey(m_ElementType))
                    _typeOptionsByType[m_ElementType] = TypeCache.GetTypesDerivedFrom(m_ElementType);
                
                foreach (var t in _typeOptionsByType[m_ElementType])
                {
                    if (!t.IsAbstract)
                        options.Add(t);
                }
                this.m_AddOptionTypes = options;
            }
            else
            {
                this.m_AddOptionTypes = Array.Empty<Type>();
            }
        }

        protected override void OnDrawHeader(Rect rect, GUIContent label)
        {
            if (GUI.enabled && displayAdd && _isUnityType)
            {
                if (eUtility.DropZone(m_ElementType, out Object[] items, rect))
                {
                    foreach (var item in items)
                        s_Defaults.DoAddButton(this, item);
                }
            }

            if (rect.IsValid())
                _headerRect = rect;
            
            GUILayout.BeginArea(_headerRect);
            GUILayout.BeginHorizontal();
            
            var drawnLabel = ValidateLabel(label);
            GUILayout.Label(drawnLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{count} Items");

            if (List != null)
            {
                var maxPagesCount = _maxPagesCount;

                if (maxPagesCount > 1 && _drawPageIndex >= maxPagesCount)
                    _drawPageIndex = maxPagesCount - 1;
            }

            if (List != null && List.Count > GetListDrawCount())
            {
                CustomEditorGUI.VerticalLine(CustomGUIStyles.LightBorderColor);

                var maxPagesCount = _maxPagesCount;

                GUILayout.Label($"{_drawPageIndex + 1}/{maxPagesCount}");

                var wasEnabled = GUI.enabled;
                GUI.enabled = _drawPageIndex > 0;
                if (CustomEditorGUI.IconButton(UnityIcon.InternalIcon("ArrowNavigationLeft")))
                {
                    bool leftClick = Event.current.button == 0;
                    if (leftClick && _drawPageIndex > 0)
                        --_drawPageIndex;
                    else
                        _drawPageIndex = 0;
                }

                GUI.enabled = _drawPageIndex < maxPagesCount - 1;
                if (CustomEditorGUI.IconButton(UnityIcon.InternalIcon("ArrowNavigationRight")))
                {
                    bool leftClick = Event.current.button == 0;
                    if (leftClick && _drawPageIndex < maxPagesCount - 1)
                        ++_drawPageIndex;
                    else
                        _drawPageIndex = maxPagesCount - 1;
                }
                GUI.enabled = wasEnabled;
            }

            if (displayAdd && GUI.enabled)
            {
                CustomEditorGUI.VerticalLine(CustomGUIStyles.LightBorderColor);
                GUILayout.Space(CustomGUIUtility.Padding * 2);
            
                using (new EditorGUI.DisabledScope(onCanAddCallback != null && !onCanAddCallback(this)))
                {
                    if (CustomEditorGUI.IconButton(_addContent, null, 16, 16))
                    {
                        OnAddElement(new Rect());

                        if (onChangedCallback != null)
                            onChangedCallback(this);

                        m_SerializedObject?.ApplyModifiedProperties();
                    }
                }
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private GUIContent ValidateLabel(GUIContent label)
        {
            if (label == null || label == GUIContent.none)
            {
                if (SerializedProperty != null)
                    return GUIContentHelper.TempContent(m_ElementsProperty.displayName);
                else if (_hostInfo != null)
                    return GUIContentHelper.TempContent(_hostInfo.NiceName);
            }
            
            return label;
        }

        protected override void DrawElement(Rect contentRect, int elementIndex, bool selected = false, bool focused = false)
        {
            if (MaxItemsPerPage > 0 && elementIndex > MaxItemsPerPage)
                return;

            Rect removeBtnRect = default;
            bool drawRemoveButton = this.displayRemove && GUI.enabled;
            if (drawRemoveButton && contentRect.IsValid())
            {
                removeBtnRect = contentRect.AlignRight(18).AlignCenterVertical(18);
                removeBtnRect.xMin += 6;
                contentRect.xMax -= 18;
            }

            base.DrawElement(contentRect, elementIndex + _drawPageIndex * MaxItemsPerPage, selected, focused);
            
            if (drawRemoveButton)
            {
                while (_closeIcons.Count <= elementIndex) // Each rect needs its own icon (cause the rect is cached)
                    _closeIcons.Add(new HoverTexture(UnityIcon.AssetIcon("Fa_Times")));

                if (CustomEditorGUI.IconButton(removeBtnRect, _closeIcons[elementIndex], tooltip: "Remove entry."))
                {
                    this.SelectedIndex = elementIndex + _drawPageIndex * MaxItemsPerPage;
                    HandleRemoveElement(this.SelectedIndex);
                    onChangedCallback?.Invoke(this);
                    if (_drawPageIndex * MaxItemsPerPage >= this.count && _drawPageIndex > 0)
                        --_drawPageIndex;
                    GUIUtility.ExitGUI();
                }
            }
        }

        protected override void CheckIfHovering(Rect contentRect, int elementIndex)
        {
            if (MaxItemsPerPage > 0)
                elementIndex %= MaxItemsPerPage;
            base.CheckIfHovering(contentRect, elementIndex);
        }


        protected override void HandleRemoveElement(int indexToRemove)
        {
            if (SerializedProperty != null)
            {
                SerializedProperty.DeleteArrayElementAtIndex(indexToRemove);
                if (SelectedIndex >= SerializedProperty.arraySize - 1)
                    SelectedIndex = SerializedProperty.arraySize - 1;
            }
            else
            {
                var collection = m_ElementList;
                if (collection is Array arr)
                {
                    m_ElementList = RemoveAtGeneric(arr, indexToRemove);
                    if (_hostInfo != null)
                        _hostInfo.TrySetValue(m_ElementList);
                }
                else
                {
                    m_ElementList.RemoveAt(indexToRemove);
                    if (_hostInfo != null)
                        _hostInfo.TrySetValue(m_ElementList);
                }
                if (SelectedIndex >= List.Count - 1)
                    SelectedIndex = List.Count - 1;
            }
        }

        protected override void OnDrawElementBackground(Rect rect, int index, bool selected, bool focused, bool draggable)
        {
            if (MaxItemsPerPage > 0 && index > MaxItemsPerPage)
                return;

            base.OnDrawElementBackground(rect, index, selected, focused, draggable);
        }

        protected override Rect DoLayoutFooter()
        {
            // Don't draw footer
            return Rect.zero;
        }

        protected override int GetListDrawCount()
        {
            if (MaxItemsPerPage > 0)
                return Mathf.Min(Mathf.Min(base.GetListDrawCount() - _drawPageIndex * MaxItemsPerPage, MaxItemsPerPage), base.GetListDrawCount());
            return base.GetListDrawCount();
        }

        protected override void OnAddElement(Rect rect)
        {
            if (!HasMultipleTypeOptions || _isUnityType)
            {
                base.OnAddElement(rect);
                return;
            }
            
            var genericMenu = new GenericMenu();
            foreach (var option in this.m_AddOptionTypes)
            {
                genericMenu.AddItem(new GUIContent(option.Name), false, () =>
                {
                    if (!Defaults.TryCreateElement(option, out object element, out string errorString))
                    {
                        Debug.LogError(errorString);
                        return;
                    }
                    
                    if (SerializedProperty != null)
                    {
                        ++SerializedProperty.arraySize;
                        var serializedPropElement = SerializedProperty.GetArrayElementAtIndex(SerializedProperty.arraySize - 1);
                        var hostInfo = serializedPropElement.GetHostInfo();
                        hostInfo.SetValue(element);
                    }
                    else
                    {
                        if (m_ElementList == null)
                            m_ElementList = (IList)Activator.CreateInstance(this.m_ListType);

                        if (m_ElementList is Array)
                        {
                            m_ElementList = (IList) ResizeArray(m_ElementList, List.Count + 1);
                            if (_hostInfo != null)
                                _hostInfo.TrySetValue(m_ElementList);
                            SelectedIndex = m_ElementList.Count - 1;

                            if (_hostInfo != null)
                            {
                                var hostInfo = _hostInfo.CreateArrayElement(SelectedIndex);
                                hostInfo.SetValue(element);
                            }
                            else 
                                m_ElementList[SelectedIndex] = element;
                        }
                        else
                        {
                            SelectedIndex = m_ElementList.Add(element);
                        
                            if (_hostInfo != null)
                                _hostInfo.TrySetValue(m_ElementList);
                        }
                    }

                    if (onChangedCallback != null)
                        onChangedCallback.Invoke(this);
                });
            }
            genericMenu.DropDown(rect);
        }
        
        static object ResizeArray(object array, int n)
        {
            var type = array.GetType();
            var elemType = type.GetElementType();
            var resizeMethod = typeof(Array).GetMethod("Resize", BindingFlags.Static | BindingFlags.Public);
            var properResizeMethod = resizeMethod.MakeGenericMethod(elemType);
            var parameters = new object[] { array, n };
            properResizeMethod.Invoke(null, parameters);
            array = parameters[0];
            return array;
        }

        protected override GUIContent GetAddIcon()
        {
            if (HasMultipleTypeOptions && !this.m_ElementType.InheritsFrom<UnityEngine.Object>())
                return s_Defaults.iconToolbarPlusMore;
            return base.GetAddIcon();
        }
    }
}