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
        private bool _clickedHeader;
        
        public override object SelectedItem => List[SelectedIndex + MaxItemsPerPage * _drawPageIndex];


        // Each tracks their own rect so you do need multiple
        private readonly List<HoverTexture> _closeIcons = new List<HoverTexture>();

        private ValidRect _headerRect;
        private GUIContent _addContent;

        private static Dictionary<Type, TypeCache.TypeCollection> _typeOptionsByType = new Dictionary<Type, TypeCache.TypeCollection>();
        private static Dictionary<Type, bool> _isValidByType = new Dictionary<Type, bool>();

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
        
        public PageableReorderableList(IList elements, bool selectableItems)
            : base(elements, selectableItems)
        {
        }
        
        public PageableReorderableList(SerializedProperty elements, bool selectableItems)
            : base(elements, selectableItems)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
        }

        public PageableReorderableList(GenericHostInfo hostInfo, bool selectableItems)
            : base(selectableItems)
        {
            MaxItemsPerPage = DEFAULT_ITEMS_PER_PAGE;
            _hostInfo = hostInfo;

            var list = _hostInfo.GetSmartValue<IList>();
            if (list == null)
            {
                var listType = _hostInfo.GetReturnType();
                // TODO: if we have an interface this will fail
                list = (IList) listType.CreateInstance();
            }
            
            Initialize(null, list);
        }

        protected override void Initialize(SerializedProperty property, IList list)
        {
            base.Initialize(property, list);

            _isUnityType = m_ElementType != null && m_ElementType.InheritsFrom<Object>();
            _addContent = new GUIContent(UnityIcon.AssetIcon("Fa_Plus"), tooltip: "Add Item");
            
            if (this.DisplayAdd && this.m_ElementType != null)
            {
                var options = new HashSet<Type>();
                if (!m_ElementType.IsAbstract)
                    options.Add(this.m_ElementType);
                if (!_typeOptionsByType.ContainsKey(m_ElementType))
                    _typeOptionsByType[m_ElementType] = TypeCache.GetTypesDerivedFrom(m_ElementType);
                
                foreach (var t in _typeOptionsByType[m_ElementType])
                {
                    if (!_isValidByType.TryGetValue(t, out bool isValid))
                    {
                        // NOTE: This is still not supported in Unity 2021, maybe they will add support in the future
                        isValid = !t.IsGenericType;// && t.ContainsGenericParameters;
                        if (t.IsAbstract)
                            isValid = false;
                        _isValidByType[t] = isValid;
                    }
                    
                    if (!isValid)
                        continue;
                    
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
            if (!DisplayHeader) return;
            
            if (GUI.enabled && DisplayAdd && _isUnityType)
            {
                if (eUtility.DropZone(m_ElementType, out Object[] items, rect))
                {
                    foreach (var item in items)
                        Add(item);
                }
            }

            if (_headerRect.Update(rect))
                RequestRepaint();
            else if (!_headerRect.IsValid())
                RequestRepaint();
            
            GUILayout.BeginArea(_headerRect);
            var areaRect = EditorGUILayout.BeginHorizontal();

            var drawnLabel = ValidateLabel(label);

            if (Collapsible)
            {
                bool wasEnabled = GUI.enabled;
                GUI.enabled = true;

                // m_Expanded = eUtility.Foldout(m_Expanded, drawnLabel);
                // eUtility.Foldout does not work properly
                // Likely due to indent stuff TODO: find out what's going on
# if UNITY_2021_OR_NEWER
                var icon = m_Expanded ? "IN_foldout_on" : "IN_foldout";
#else
                var icon = m_Expanded ? "IN foldout on" : "IN foldout";
#endif
                if (CustomEditorGUI.IconButton(UnityIcon.InternalIcon(icon)))
                    SetExpanded(!m_Expanded);
                else
                {
                    var width = GUI.skin.label.CalcMaxWidth(drawnLabel);
                    width += 30; // approx the width of the iconbutton
                    if (eUtility.IsClicked(ref _clickedHeader, areaRect.AlignLeft(width)))
                        SetExpanded(!m_Expanded);
                }
                
                GUILayout.Label(drawnLabel);
                
                GUI.enabled = wasEnabled;
            }
            else
                GUILayout.Label(drawnLabel);
            
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{count} Items");

            if (List != null)
            {
                var maxPagesCount = _maxPagesCount;

                // If at a page beyond the max, return to the last valid page
                if (_drawPageIndex >= maxPagesCount && maxPagesCount > 0)
                    _drawPageIndex = maxPagesCount - 1;
            }

            if (List != null && List.Count > GetListDrawCount())
            {
                CustomEditorGUI.VerticalLine(CustomGUIStyles.LightBorderColor);

                var maxPagesCount = _maxPagesCount;

                GUILayout.Label($"{_drawPageIndex + 1}/{maxPagesCount}");

                bool wasEnabled = GUI.enabled;
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

            if (DisplayAdd && GUI.enabled)
            {
                CustomEditorGUI.VerticalLine(CustomGUIStyles.LightBorderColor);
                GUILayout.Space(CustomGUIUtility.Padding * 2);
            
                using (new EditorGUI.DisabledScope(onCanAddCallback != null && !onCanAddCallback(this)))
                {
                    if (CustomEditorGUI.IconButton(_addContent, null, 16, 16))
                    {
                        var addElementRect = new Rect(0, 0, _headerRect.width, _headerRect.height);
                        OnAddElement(addElementRect);

                        m_SerializedObject?.ApplyModifiedProperties();
                        
                        if (onChangedCallback != null)
                            onChangedCallback(this);
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private GUIContent ValidateLabel(GUIContent label)
        {
            if (label == null || label == GUIContent.none)
            {
                if (SerializedProperty != null)
                    return GUIContentHelper.TempContent(m_SerializedProperty.displayName);
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
            bool drawRemoveButton = this.DisplayRemove && GUI.enabled;
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
                    int actualIndex = elementIndex + _drawPageIndex * MaxItemsPerPage;
                    if (SelectableItems)
                        this.SelectedIndex = actualIndex;
                    HandleRemoveElement(actualIndex);
                    onChangedCallback?.Invoke(this);
                    if (_drawPageIndex * MaxItemsPerPage >= this.count && _drawPageIndex > 0)
                        --_drawPageIndex;
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
                if (SelectableItems && SelectedIndex >= SerializedProperty.arraySize - 1)
                    SelectedIndex = SerializedProperty.arraySize - 1;

                m_SerializedObject.ApplyModifiedProperties();
            }
            else
            {
                var collection = m_ElementList;
                if (collection is Array arr)
                {
                    m_ElementList = arr.RemoveAtGeneric(indexToRemove);
                    _hostInfo?.TrySetValue(m_ElementList);
                }
                else
                {
                    m_ElementList.RemoveAt(indexToRemove);
                    _hostInfo?.TrySetValue(m_ElementList);
                }
                
                if (SelectableItems && SelectedIndex >= List.Count - 1)
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
        
        protected override void DoListFooter(Rect footerRect)
        {
            // Don't draw footer
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
                genericMenu.AddItem(new GUIContent(option.GetNiceName(false)), false, () =>
                {
                    if (!Defaults.TryCreateElement(option, out object element, out string errorString))
                    {
                        Debug.LogError(errorString);
                        return;
                    }

                    Add(element);
                    
                    if (onChangedCallback != null)
                        onChangedCallback.Invoke(this);
                });
            }
            genericMenu.DropdownLeft(rect);
        }

        protected override int Add(object element)
        {
            var index = base.Add(element);
            
            // If we have a SerializedProperty this should have already been handled
            // If we have a RootHostInfo we can't set
            if (SerializedProperty == null && !(_hostInfo is RootHostInfo))
                _hostInfo.TrySetValue(m_ElementList);
            // TODO handle structs

            return index;
        }

        protected override void SetArrayElement(int newIndex, object element)
        {
            base.SetArrayElement(newIndex, element);
            // The array reference changed (due to resizing)
            // It will only get here when there is no SerializedProperty
            if (_hostInfo is RootHostInfo rootInfo)
                Debug.LogWarning("Cannot change reference of array, how do we handle this?");
            else 
                _hostInfo.TrySetValue(m_ElementList);
        }

        protected override GUIContent GetAddIcon()
        {
            if (HasMultipleTypeOptions && !this.m_ElementType.InheritsFrom<UnityEngine.Object>())
                return s_Defaults.iconToolbarPlusMore;
            return base.GetAddIcon();
        }

        public bool MovePage(int amount)
        {
            int oldIndex = _drawPageIndex;
            _drawPageIndex += amount;
            _drawPageIndex = Math.Min(_maxPagesCount - 1, Math.Max(0, _drawPageIndex));
            return oldIndex != _drawPageIndex;
        }
    }
}