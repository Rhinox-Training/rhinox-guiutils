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

namespace Rhinox.GUIUtils.Editor
{
    public class PageableReorderableList : BetterReorderableList
    {
        public int MaxItemsPerPage { get; set; } = DEFAULT_ITEMS_PER_PAGE;
        private const int DEFAULT_ITEMS_PER_PAGE = 100;
        
        private ICollection<Type> m_AddOptionTypes;
        private int _drawPageIndex;

        private static Dictionary<Type, TypeCache.TypeCollection> _typeOptionsByType = new Dictionary<Type, TypeCache.TypeCollection>();

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

        public string CustomTitle { get; private set; }

        public PageableReorderableList(IList elements, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true) 
            : base(elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            CustomTitle = elements.GetType().Name;
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
        }

        public PageableReorderableList(SerializedObject serializedObject, SerializedProperty elements, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true) 
            : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
        }

        public PageableReorderableList(object containerInstance, MemberInfo memberInfo, 
            bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
            : base(memberInfo.GetValue(containerInstance) as IList, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
            CustomTitle = memberInfo.Name;
        }

        protected override void InitList(SerializedObject serializedObject, SerializedProperty elements, IList elementList, bool draggable,
            bool displayHeader, bool displayAddButton, bool displayRemoveButton)
        {
            base.InitList(serializedObject, elements, elementList, draggable, displayHeader, displayAddButton, displayRemoveButton);
            
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

        protected override void OnDrawHeader(Rect rect)
        {
            var nameRect = rect.AlignLeft(rect.width  * 0.6f);
            var secondaryRect = rect.AlignRight(rect.width * 0.4f);
            var sizeRect = secondaryRect.AlignLeft(secondaryRect.width * 0.5f);
            var multiPageRect = secondaryRect.AlignRight(secondaryRect.width * 0.5f);
            if (serializedProperty != null)
                EditorGUI.LabelField(nameRect, this.serializedProperty.displayName);
            else
                EditorGUI.LabelField(nameRect, CustomTitle);
            EditorGUI.LabelField(sizeRect, $"{count} Items");

            if (list != null && list.Count > GetListDrawCount())
            {
                var maxPagesCount = Mathf.CeilToInt((float)list.Count / MaxItemsPerPage);
                var infoRect = multiPageRect.AlignLeft(multiPageRect.width * 0.4f);
                EditorGUI.LabelField(infoRect, $"{_drawPageIndex + 1}/{maxPagesCount}");
                var buttonsRect = multiPageRect.AlignRight(multiPageRect.width * 0.6f);
                var leftButtonRect = buttonsRect.AlignLeft(buttonsRect.width * 0.5f);
                var rightButtonRect = buttonsRect.AlignRight(buttonsRect.width * 0.5f);
                EditorGUI.BeginDisabledGroup(_drawPageIndex <= 0);
                if (GUI.Button(leftButtonRect, "<"))
                {
                    if (_drawPageIndex > 0)
                        --_drawPageIndex;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(_drawPageIndex >= maxPagesCount - 1);
                if (GUI.Button(rightButtonRect, ">"))
                {
                    if (_drawPageIndex < maxPagesCount - 1)
                        ++_drawPageIndex;
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        protected override void DoListFooter(Rect rect)
        {
            if (!displayAdd && !displayRemove)
                return;

            if (MaxItemsPerPage > 0)
            {
                var list = serializedProperty != null ?  serializedProperty.GetValue() as IList : this.list as IList;
                if (list != null && list.Count > MaxItemsPerPage)
                    rect.y -= (list.Count - MaxItemsPerPage - 1) * elementHeight;
            }
            
            s_Defaults.DrawFooter(rect, this, this.displayAdd, false);
        }

        protected override void DrawElement(Rect contentRect, int elementIndex, bool selected = false, bool focused = false)
        {
            if (MaxItemsPerPage > 0 && elementIndex > MaxItemsPerPage)
                return;
            // const float margin = 16.0f;
            // contentRect.y += margin; // TODO: is this margin?
            // contentRect.height = elementHeight + margin;

            Rect removeButton = default;
            if (this.displayRemove)
            {
                removeButton = contentRect.AlignRight(18).AlignCenterVertical(18);
                contentRect = contentRect.PadRight(9);
            }

            base.DrawElement(contentRect, elementIndex + _drawPageIndex * MaxItemsPerPage, selected, focused);

            if (this.displayRemove)
            {
                if (GUI.Button(removeButton, GUIContentHelper.TempContent(UnityIcon.AssetIcon("Fa_Times").Pad(2), tooltip: "Remove entry.")))
                {
                    this.index = elementIndex + _drawPageIndex * MaxItemsPerPage;
                    s_Defaults.OnRemoveElement(this);
                    onChangedCallback?.Invoke(this);
                    if (_drawPageIndex * MaxItemsPerPage >= this.count)
                        --_drawPageIndex;
                }
            }
        }
        
        protected override void OnDrawElementBackground(Rect rect, int index, bool selected, bool focused, bool draggable)
        {
            if (MaxItemsPerPage > 0 && index > MaxItemsPerPage)
                return;

            s_Defaults.DrawElementBackgroundAlternating(rect, index, selected, focused, draggable);
        }
        
        protected override int GetListDrawCount()
        {
            if (MaxItemsPerPage > 0)
                return Mathf.Min(Mathf.Min(list.Count - _drawPageIndex * MaxItemsPerPage, MaxItemsPerPage), base.GetListDrawCount());
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
                    if (serializedProperty != null)
                    {
                        ++serializedProperty.arraySize;
                        var serializedPropElement = serializedProperty.GetArrayElementAtIndex(serializedProperty.arraySize - 1);
                        var hostInfo = serializedPropElement.GetHostInfo();
                        if (option.InheritsFrom(typeof(UnityEngine.Object)))
                        {
                            hostInfo.SetValue((UnityEngine.Object)null);
                        }
                        else
                        {
                            var instance = Activator.CreateInstance(option);
                            hostInfo.SetValue(instance);
                        }
                    }
                    else
                    {
                        if (list == null)
                            list = (IList)Activator.CreateInstance(this.m_ListType);
                        index = list.Add(Activator.CreateInstance(option));
                    }

                    if (onChangedCallback != null)
                        onChangedCallback.Invoke(this);
                });
            }
            genericMenu.DropDown(rect1);
        }

        protected override GUIContent GetAddIcon()
        {
            if (HasMultipleTypeOptions && !this.m_ElementType.InheritsFrom<UnityEngine.Object>())
                return s_Defaults.iconToolbarPlusMore;
            return base.GetAddIcon();
        }
    }
}