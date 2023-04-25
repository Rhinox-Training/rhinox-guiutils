using System;
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
            
            var nameRect = rect.AlignLeft(rect.width  * 0.6f);
            var secondaryRect = rect.AlignRight(rect.width * 0.4f);
            var sizeRect = secondaryRect.AlignLeft(secondaryRect.width * 0.5f);
            var multiPageRect = secondaryRect.AlignRight(secondaryRect.width * 0.5f);

            var drawnLabel = ValidateLabel(label);
            EditorGUI.LabelField(nameRect, drawnLabel);

            EditorGUI.LabelField(sizeRect, $"{count} Items");

            if (List != null && List.Count > GetListDrawCount())
            {
                var maxPagesCount = _maxPagesCount;
                var infoRect = multiPageRect.AlignLeft(multiPageRect.width * 0.4f);
                EditorGUI.LabelField(infoRect, $"{_drawPageIndex + 1}/{maxPagesCount}");
                var buttonsRect = multiPageRect.AlignRight(multiPageRect.width * 0.6f);
                var leftButtonRect = buttonsRect.AlignLeft(buttonsRect.width * 0.5f);
                var rightButtonRect = buttonsRect.AlignRight(buttonsRect.width * 0.5f);
                var wasEnabled = GUI.enabled;
                GUI.enabled = _drawPageIndex > 0;
                if (GUI.Button(leftButtonRect, "<"))
                {
                    if (_drawPageIndex > 0)
                        --_drawPageIndex;
                }
                GUI.enabled = _drawPageIndex < maxPagesCount - 1;
                if (GUI.Button(rightButtonRect, ">"))
                {
                    if (_drawPageIndex < maxPagesCount - 1)
                        ++_drawPageIndex;
                }
                GUI.enabled = wasEnabled;
            }
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

        protected override void DoListFooter(Rect rect)
        {
            if (!displayAdd && !displayRemove)
                return;

            s_Defaults.DrawFooter(rect, this, this.displayAdd, this.displayRemove, HandleRemoveElement);
        }

        protected override void DrawElement(Rect contentRect, int elementIndex, bool selected = false, bool focused = false)
        {
            if (MaxItemsPerPage > 0 && elementIndex > MaxItemsPerPage)
                return;

            // Leave a little space for easier selection
            if (displayRemove && GUI.enabled)
                contentRect.xMax -= 6;

            base.DrawElement(contentRect, elementIndex + _drawPageIndex * MaxItemsPerPage, selected, focused);
        }


        protected override void HandleRemoveElement(int indexToRemove)
        {
            if (SerializedProperty != null)
            {
                SerializedProperty.DeleteArrayElementAtIndex(indexToRemove);
                if (index >= SerializedProperty.arraySize - 1)
                    index = SerializedProperty.arraySize - 1;
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
                if (index >= List.Count - 1)
                    index = List.Count - 1;
            }
        }

        protected override void OnDrawElementBackground(Rect rect, int index, bool selected, bool focused, bool draggable)
        {
            if (MaxItemsPerPage > 0 && index > MaxItemsPerPage)
                return;

            base.OnDrawElementBackground(rect, index, selected, focused, draggable);
        }
        
        protected override int GetListDrawCount()
        {
            if (MaxItemsPerPage > 0)
                return Mathf.Min(Mathf.Min(base.GetListDrawCount() - _drawPageIndex * MaxItemsPerPage, MaxItemsPerPage), base.GetListDrawCount());
            return base.GetListDrawCount();
        }

        protected override void OnAddElement(Rect rect1)
        {
            if (!HasMultipleTypeOptions || this.m_ElementType.InheritsFrom<UnityEngine.Object>())
            {
                base.OnAddElement(rect1);
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
                            index = m_ElementList.Count - 1;

                            if (_hostInfo != null)
                            {
                                var hostInfo = _hostInfo.CreateArrayElement(index);
                                hostInfo.SetValue(element);
                            }
                            else 
                                m_ElementList[index] = element;
                        }
                        else
                        {
                            index = m_ElementList.Add(element);
                        
                            if (_hostInfo != null)
                                _hostInfo.TrySetValue(m_ElementList);
                        }
                    }

                    if (onChangedCallback != null)
                        onChangedCallback.Invoke(this);
                });
            }
            genericMenu.DropDown(rect1);
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