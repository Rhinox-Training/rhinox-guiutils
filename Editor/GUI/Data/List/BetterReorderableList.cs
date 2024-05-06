// Decompiled with JetBrains decompiler
// Type: UnityEditorInternal.ReorderableList
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FDB4E7A2-29AE-4EF7-91F6-716E71E93744
// Assembly location: D:\software\UnityEditor\2019.4.30f1\Editor\Data\Managed\UnityEditor.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class BetterReorderableList
    {
        public BetterReorderableList.ElementCallbackDelegate drawElementCallback;
        //public BetterReorderableList.GenericDelegate drawElementCountCallback;

        public BetterReorderableList.DrawNoneElementCallback drawNoneElementCallback =
            (BetterReorderableList.DrawNoneElementCallback) null;

        public BetterReorderableList.ElementHeightCallbackDelegate elementHeightCallback;
        public BetterReorderableList.ReorderCallbackDelegateWithDetails onReorderCallbackWithDetails;
        public BetterReorderableList.ReorderCallbackDelegate onReorderCallback;
        public BetterReorderableList.SelectCallbackDelegate onSelectCallback;
        public BetterReorderableList.DragCallbackDelegate onMouseDragCallback;
        public BetterReorderableList.SelectCallbackDelegate onMouseUpCallback;
        public BetterReorderableList.CanRemoveCallbackDelegate onCanRemoveCallback;
        public BetterReorderableList.CanAddCallbackDelegate onCanAddCallback;
        public BetterReorderableList.ChangedCallbackDelegate onChangedCallback;
        
        private int m_ActiveElement = -1;
        private float m_DragOffset = 0.0f;
        private ExposedGUISlideGroup m_SlideGroup;

        protected GenericHostInfo _hostInfo;
        protected SerializedObject m_SerializedObject;
        protected SerializedProperty m_SerializedProperty;
        protected IList m_ElementList;
        
        protected Type m_ListType;
        protected Type m_ElementType;

        public bool SelectableItems;
        public bool DisplayHeader;
        public bool DisplayAdd;
        public bool DisplayRemove;
        public bool Draggable;
        public bool Collapsible;
        
        private float m_DraggedY;
        private bool m_Dragging;
        protected bool m_Expanded;
        private List<int> m_NonDragTargetIndices;
        
        private int id = -1;
        protected static BetterReorderableList.Defaults s_Defaults;
        
        public float elementHeight = 21f;
        public float headerHeight = 20f;
        public float footerHeight = 20f;
        public bool showDefaultBackground = true;
        
        private float elementMargin = 4f;
        private const float kListElementBottomPadding = 4f;
        private int _elementHovering = -1;
        
        public Rect Rect { get; private set; }
        public bool Expanded => m_Expanded;
        private readonly List<Rect> _cachedRects = new List<Rect>();

        public event Action RepaintRequested;

        public static BetterReorderableList.Defaults defaultBehaviours => BetterReorderableList.s_Defaults;

        public BetterReorderableList(IList elements, bool selectableItems)
            : this(selectableItems)
        {
            this.Initialize(null, elements);
        }

        public BetterReorderableList(SerializedProperty property, bool selectableItems)
            : this(selectableItems)
        {
            this.Initialize(property, null);
        }

        protected BetterReorderableList(bool selectableItems = false)
        {
            this.SelectableItems = selectableItems;
            this.DisplayAdd = true;
            this.DisplayHeader = true;
            this.DisplayRemove = true;
            this.Draggable = true;
            this.Collapsible = true;
            this.m_Dragging = false;
            this.m_SlideGroup = new ExposedGUISlideGroup();
        }

        protected virtual void Initialize(SerializedProperty property, IList list)
        {
            this.id = CustomGUIUtility.GetPermanentControlID();
            this.m_SerializedProperty = property;
            if (property != null)
                this.m_SerializedObject = property.serializedObject;
            this.m_ElementList = list;
            footerHeight = 0;

            if (m_SerializedProperty != null)
            {
                _hostInfo = m_SerializedProperty.GetHostInfo();
                m_ElementList = _hostInfo.GetValue() as IList;
            }
            else if (_hostInfo == null)
                _hostInfo = new RootHostInfo(m_ElementList);
            
            this.m_ListType = _hostInfo.GetReturnType();
            this.m_ElementType = this.m_ListType.GetCollectionElementType();

            if (this.m_SerializedProperty != null && !this.m_SerializedProperty.editable)
                this.Draggable = false;
            if (this.m_SerializedProperty == null || this.m_SerializedProperty.isArray)
                return;
            Debug.LogError("Input elements should be an Array SerializedProperty");
        }

        public SerializedProperty SerializedProperty
        {
            get => m_SerializedProperty;
            set => m_SerializedProperty = value;
        }

        public IList List
        {
            get => m_ElementList;
            set => m_ElementList = value;
        }

        public int SelectedIndex
        {
            get => m_ActiveElement;
            set => m_ActiveElement = value;
        }

        public virtual object SelectedItem => List[m_ActiveElement];

        private float listElementTopPadding => CustomGUIUtility.Padding;

        public bool AreElementsDraggable
        {
            get => Draggable;
            set => Draggable = value;
        }

        private Rect GetContentRect(Rect rect)
        {
            if (!rect.IsValid())
                return rect;

            Rect contentRect = rect;
            if (AreElementsDraggable && GUI.enabled)
                contentRect.xMin += 20f;
            else
                contentRect.xMin += 6f;
            contentRect.xMax -= 6f;
            
            return contentRect;
        }

        private float GetElementYOffset(int index) => GetElementYOffset(index, -1);

        private float GetElementYOffset(int index, int skipIndex)
        {
            // if (this.elementHeightCallback == null)
            //     return (float) index * this.elementHeight;
            float elementYoffset = 0.0f;
            for (int i = 0; i < index; ++i)
            {
                if (i != skipIndex)
                    elementYoffset += GetElementHeight(i);
            }

            return elementYoffset + (elementMargin / 2.0f);
        }

        protected virtual float GetElementHeight(int index)
        {
            if (this.elementHeightCallback != null)
                return this.elementHeightCallback(index) + this.elementMargin;
            return this.elementHeight + this.elementMargin;
        }

        private Rect GetRowRect(int index, Rect listRect) => new Rect(listRect.x,
            listRect.y + this.GetElementYOffset(index), listRect.width, this.GetElementHeight(index));

        public int count
        {
            get
            {
                if (this.m_SerializedProperty == null)
                    return this.m_ElementList.Count;
                if (!this.m_SerializedProperty.hasMultipleDifferentValues)
                    return this.m_SerializedProperty.arraySize;
                int val2 = this.m_SerializedProperty.arraySize;
                foreach (UnityEngine.Object targetObject in this.m_SerializedProperty.serializedObject.targetObjects)
                    val2 = Math.Min(
                        new SerializedObject(targetObject).FindProperty(this.m_SerializedProperty.propertyPath).arraySize, val2);
                return val2;
            }
        }

        public void DoLayoutList(GUIContent label)
        {
            GUILayout.BeginVertical();

            if (BetterReorderableList.s_Defaults == null)
                BetterReorderableList.s_Defaults = new BetterReorderableList.Defaults();

            var headerRect = DoLayoutHeader(label);
            var combinedRect = new Rect(headerRect);

            if (!Collapsible || Expanded)
            {
                var elementsRect = DoLayoutElements();
                if (elementsRect.IsValid())
                    combinedRect.height += elementsRect.height;
            }
            
            var footerRect = DoLayoutFooter();
            if (footerRect.IsValid())
                combinedRect.height += footerRect.height;
            
            if (combinedRect.IsValid())
                Rect = combinedRect;
            
            GUILayout.EndVertical();
        }

        protected virtual Rect DoLayoutHeader(GUIContent label)
        {
            if (!DisplayHeader)
                return Rect.zero;
            
            Rect rect = GUILayoutUtility.GetRect(0.0f, this.headerHeight, GUILayout.ExpandWidth(true));
            this.DoListHeader(rect, label);
            return rect;
        }
        
        protected virtual Rect DoLayoutElements()
        {
            Rect rect = GUILayoutUtility.GetRect(10f, this.GetListElementHeight(), GUILayout.ExpandWidth(true));
            this.DoListElements(rect);
            return rect;
        }

        protected virtual Rect DoLayoutFooter()
        {
            if ((!DisplayAdd && !DisplayRemove) || !GUI.enabled)
                return Rect.zero;

            Rect rect = GUILayoutUtility.GetRect(4f, this.footerHeight, GUILayout.ExpandWidth(true));
            this.DoListFooter(rect);
            return rect;
        }

        public void DoList(Rect rect, GUIContent label)
        {
            if (BetterReorderableList.s_Defaults == null)
                BetterReorderableList.s_Defaults = new BetterReorderableList.Defaults();

            Rect = rect;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 0.0f);
            if (DisplayHeader && rect.IsValid())
                headerRect = new Rect(rect.x, rect.y, rect.width, this.headerHeight);
            
            Rect footerRect;
            this.DoListHeader(headerRect, label);
            if (!Collapsible || Expanded)
            {
                Rect listRect = new Rect(rect.x, headerRect.y + headerRect.height, rect.width, this.GetListElementHeight());
                this.DoListElements(listRect);
                footerRect = new Rect(rect.x, listRect.y + listRect.height, rect.width, this.footerHeight);
            }
            else
            {
                footerRect = new Rect(rect.x, headerRect.y + headerRect.height, rect.width, this.footerHeight);
            }
            this.DoListFooter(footerRect);
        }

        public float GetHeight()
        {
            var height = this.GetListElementHeight() + this.footerHeight;
            if (DisplayHeader)
                height += headerHeight;
            return height;
        }

        private float GetListElementHeight()
        {
            if (Collapsible && !Expanded)
                return 0;
                
            float num = 4f + this.listElementTopPadding;
            int count = GetListDrawCount();
            if (count == 0)
                return this.elementHeight + num;

            float lastElementPos = this.GetElementYOffset(count - 1);
            float lastElementHeight = this.GetElementHeight(count - 1);
            float total = lastElementPos + lastElementHeight + num;
            return total;
        }

        private void DoListElements(Rect listRect)
        {
            int count = GetListDrawCount();
            if (this.showDefaultBackground && Event.current.type == UnityEngine.EventType.Repaint)
                BetterReorderableList.s_Defaults.boxBackground.Draw(listRect, false, false, false, false);

            if (listRect.IsValid())
            {
                listRect.yMin += this.listElementTopPadding;
                listRect.yMax -= 4f;
                if (this.showDefaultBackground)
                {
                    ++listRect.xMin;
                    --listRect.xMax;
                }
            }

            Rect elementRect = listRect;
            if (elementRect.IsValid())
                elementRect.height = this.elementHeight;

            if ((this.m_SerializedProperty != null && this.m_SerializedProperty.isArray || this.m_ElementList != null) && count > 0)
            {
                if (this.IsDragging() && Event.current.type == UnityEngine.EventType.Repaint)
                {
                    int rowIndex = this.CalculateRowIndex();
                    this.m_NonDragTargetIndices.Clear();
                    for (int index = 0; index < count; ++index)
                    {
                        if (index != this.m_ActiveElement)
                            this.m_NonDragTargetIndices.Add(index);
                    }

                    this.m_NonDragTargetIndices.Insert(rowIndex, -1);
                    bool flag = false;
                    for (int index = 0; index < this.m_NonDragTargetIndices.Count; ++index)
                    {
                        if (this.m_NonDragTargetIndices[index] != -1)
                        {
                            if (elementRect.IsValid())
                            {
                                elementRect.height = this.GetElementHeight(index);
                                elementRect.y = listRect.y + this.GetElementYOffset(this.m_NonDragTargetIndices[index],
                                    this.m_ActiveElement);
                                if (flag)
                                    elementRect.y += GetElementHeight(this.m_ActiveElement);
                                elementRect = this.m_SlideGroup.GetRect(this.m_NonDragTargetIndices[index], elementRect);
                            }
                            OnDrawElementBackground(elementRect, index, false, false, Draggable);
                            
                            s_Defaults.DrawElementDraggingHandle(elementRect, index, false, false, this.Draggable);
                            Rect contentRect = this.GetContentRect(elementRect);
                            
                            DrawElement(contentRect, this.m_NonDragTargetIndices[index]);
                        }
                        else
                            flag = true;
                    }

                    if (elementRect.IsValid())
                        elementRect.y = this.m_DraggedY - this.m_DragOffset + listRect.y;
                    
                    OnDrawElementBackground(elementRect, this.m_ActiveElement, true, true, Draggable);
                    s_Defaults.DrawElementDraggingHandle(elementRect, this.m_ActiveElement, true, true, this.Draggable);
                    Rect contentRect1 = this.GetContentRect(elementRect);

                    DrawElement(contentRect1, m_ActiveElement, true, true);
                }
                else // any event except when we're dragging in repaint
                {
                    for (int index = 0; index < count; ++index)
                    {
                        bool isSelected = index == this.m_ActiveElement;
                        bool isFocused = index == this.m_ActiveElement && this.HasKeyboardControl();
                        if (elementRect.IsValid())
                        {
                            elementRect.height = this.GetElementHeight(index);
                            elementRect.y = listRect.y + this.GetElementYOffset(index);
                        }

                        OnDrawElementBackground(elementRect, index, isSelected, isFocused, this.Draggable);
                        s_Defaults.DrawElementDraggingHandle(elementRect, index, isSelected, isFocused, this.Draggable);
                        Rect contentRect = this.GetContentRect(elementRect);
                        
                        DrawElement(contentRect, index, isSelected, isFocused);
                    }
                }

                this.DoDraggingAndSelection(listRect);
            }
            else // if there are no elements
            {
                elementRect.y = listRect.y;
                OnDrawElementBackground(elementRect, -1, false, false, false);
                BetterReorderableList.s_Defaults.DrawElementDraggingHandle(elementRect, -1, false, false, false);
                Rect rect2 = elementRect;
                rect2.xMin += 6f;
                rect2.xMax -= 6f;
                if (this.drawNoneElementCallback == null)
                    BetterReorderableList.s_Defaults.DrawNoneElement(rect2, this.Draggable);
                else
                    this.drawNoneElementCallback(rect2);
            }
        }

        protected virtual int GetListDrawCount()
        {
            int count = this.count;
            return count;
        }

        protected virtual void DrawElement(Rect contentRect, int elementIndex, bool selected = false, bool focused = false)
        {
            if (!contentRect.IsValid() && _cachedRects.HasIndex(elementIndex))
                contentRect = _cachedRects[elementIndex];
            else
            {
                while (_cachedRects.Count <= elementIndex)
                    _cachedRects.Add(Rect.zero);
            
                _cachedRects[elementIndex] = contentRect;
                CheckIfHovering(contentRect, elementIndex);
            }

            if (this.drawElementCallback == null)
            {
                if (this.m_SerializedProperty != null)
                    s_Defaults.DrawElement(contentRect,
                        this.m_SerializedProperty.GetArrayElementAtIndex(elementIndex),
                        (object) null, 
                        selected, focused, 
                        this.Draggable);
                else
                    s_Defaults.DrawElement(contentRect, 
                        (SerializedProperty) null,
                        this.m_ElementList[elementIndex], 
                        selected, focused,
                        this.Draggable);
            }
            else
                this.drawElementCallback(contentRect, elementIndex, selected, focused);
        }

        protected virtual void CheckIfHovering(Rect contentRect, int elementIndex)
        {
            var e = Event.current;
            if (!eUtility.IsMouseOver(contentRect, e))
                return;
            
            if (_elementHovering != elementIndex)
            {
                _elementHovering = elementIndex;
                RequestRepaint();
            }
        }

        protected virtual void OnDrawElementBackground(Rect rect, int index, bool selected, bool focused, bool draggable)
        {
            s_Defaults.DrawElementBackground(rect, index, selected, focused, _elementHovering == index, draggable);
        }

        private void DoListHeader(Rect headerRect, GUIContent label)
        {
            if (this.showDefaultBackground && Event.current.type == UnityEngine.EventType.Repaint)
                BetterReorderableList.s_Defaults.DrawHeaderBackground(headerRect);
            if (headerRect.IsValid())
            {
                headerRect.xMin += 6f;
                headerRect.xMax -= 6f;
                headerRect.height -= 2f;
                ++headerRect.y;
            }
            
            OnDrawHeader(headerRect, label);
        }

        protected virtual void OnDrawHeader(Rect headerRect, GUIContent label)
        {
            if (!this.DisplayHeader)
                return;
            
            if (Collapsible)
            {
                var expanded = s_Defaults.DrawHeader(headerRect, m_Expanded, this.m_SerializedProperty, this.m_ElementList);
                SetExpanded(expanded);
            }
            else
                s_Defaults.DrawHeader(headerRect, this.m_SerializedProperty, this.m_ElementList);
        }
        
        protected virtual void OnAddElement(Rect rect)
        {
            if (s_Defaults.TryCreateElement(this, out object item, out string error))
                Add(item);
            else
                Debug.LogError(error);
        }

        protected virtual void DoListFooter(Rect footerRect)
        {
            s_Defaults.DrawFooter(footerRect, this, this.DisplayAdd, this.DisplayRemove, HandleRemoveElement);
        }

        protected virtual void HandleRemoveElement(int indexToRemove)
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
                }
                else
                {
                    m_ElementList.RemoveAt(indexToRemove);
                }
                if (SelectableItems && SelectedIndex >= List.Count - 1)
                    SelectedIndex = List.Count - 1;
            }
        }
        
        private void DoDraggingAndSelection(Rect listRect)
        {
            Event current = Event.current;
            int activeElement1 = this.m_ActiveElement;
            bool flag = false;
            switch (current.GetTypeForControl(this.id))
            {
                case UnityEngine.EventType.MouseDown:
                    if (listRect.Contains(Event.current.mousePosition) &&
                        (Event.current.button == 0 || Event.current.button == 1))
                    {
                        CustomEditorGUI.EndEditingActiveTextField();
                        if (SelectableItems)
                            this.m_ActiveElement = this.GetRowIndex(Event.current.mousePosition.y - listRect.y);
                        if (this.Draggable && Event.current.button == 0)
                        {
                            this.m_DragOffset = Event.current.mousePosition.y - listRect.y -
                                                this.GetElementYOffset(this.m_ActiveElement);
                            this.UpdateDraggedY(listRect);
                            GUIUtility.hotControl = this.id;
                            this.m_SlideGroup.Reset();
                            this.m_NonDragTargetIndices = new List<int>();
                        }

                        this.GrabKeyboardFocus();
                        if (Event.current.button != 1)
                        {
                            current.Use();
                            flag = true;
                            break;
                        }

                        break;
                    }

                    break;
                case UnityEngine.EventType.MouseUp:
                    if (!this.Draggable)
                    {
                        if (this.onMouseUpCallback != null && this.IsMouseInsideActiveElement(listRect))
                        {
                            this.onMouseUpCallback(this);
                            break;
                        }

                        break;
                    }

                    if (GUIUtility.hotControl == this.id)
                    {
                        current.Use();
                        this.m_Dragging = false;
                        try
                        {
                            int rowIndex = this.CalculateRowIndex();
                            if (this.m_ActiveElement != rowIndex)
                            {
                                if (this.m_SerializedObject != null && this.m_SerializedProperty != null)
                                {
                                    this.m_SerializedProperty.MoveArrayElement(this.m_ActiveElement, rowIndex);
                                    this.m_SerializedObject.ApplyModifiedProperties();
                                    this.m_SerializedObject.Update();
                                }
                                else if (this.m_ElementList != null)
                                {
                                    object element = this.m_ElementList[this.m_ActiveElement];
                                    for (int index = 0; index < this.m_ElementList.Count - 1; ++index)
                                    {
                                        if (index >= this.m_ActiveElement)
                                            this.m_ElementList[index] = this.m_ElementList[index + 1];
                                    }

                                    for (int index = this.m_ElementList.Count - 1; index > 0; --index)
                                    {
                                        if (index > rowIndex)
                                            this.m_ElementList[index] = this.m_ElementList[index - 1];
                                    }

                                    this.m_ElementList[rowIndex] = element;
                                }

                                int activeElement2 = this.m_ActiveElement;
                                int newIndex = rowIndex;
                                this.m_ActiveElement = rowIndex;
                                if (this.onReorderCallbackWithDetails != null)
                                    this.onReorderCallbackWithDetails(this, activeElement2, newIndex);
                                else if (this.onReorderCallback != null)
                                    this.onReorderCallback(this);
                                if (this.onChangedCallback != null)
                                {
                                    this.onChangedCallback(this);
                                    break;
                                }

                                break;
                            }

                            if (this.onMouseUpCallback != null)
                                this.onMouseUpCallback(this);
                            break;
                        }
                        finally
                        {
                            GUIUtility.hotControl = 0;
                            this.m_NonDragTargetIndices = (List<int>) null;
                        }
                    }
                    else
                        break;
                case UnityEngine.EventType.MouseDrag:
                    if (this.Draggable && GUIUtility.hotControl == this.id)
                    {
                        this.m_Dragging = true;
                        if (this.onMouseDragCallback != null)
                            this.onMouseDragCallback(this);
                        this.UpdateDraggedY(listRect);
                        current.Use();
                        break;
                    }

                    break;
                case UnityEngine.EventType.KeyDown:
                    if (GUIUtility.keyboardControl != this.id)
                        return;
                    if (SelectableItems && current.keyCode == KeyCode.DownArrow)
                    {
                        ++this.m_ActiveElement;
                        current.Use();
                    }

                    if (SelectableItems && current.keyCode == KeyCode.UpArrow)
                    {
                        --this.m_ActiveElement;
                        current.Use();
                    }

                    if (current.keyCode == KeyCode.Escape && GUIUtility.hotControl == this.id)
                    {
                        GUIUtility.hotControl = 0;
                        this.m_Dragging = false;
                        current.Use();
                    }

                    this.m_ActiveElement = Mathf.Clamp(this.m_ActiveElement, 0,
                        this.m_SerializedProperty != null ? this.m_SerializedProperty.arraySize - 1 : this.m_ElementList.Count - 1);
                    break;
            }

            if (!(this.m_ActiveElement != activeElement1 | flag) || this.onSelectCallback == null)
                return;
            this.onSelectCallback(this);
        }

        private bool IsMouseInsideActiveElement(Rect listRect)
        {
            int rowIndex = this.GetRowIndex(Event.current.mousePosition.y - listRect.y);
            return rowIndex == this.m_ActiveElement &&
                   this.GetRowRect(rowIndex, listRect).Contains(Event.current.mousePosition);
        }

        private void UpdateDraggedY(Rect listRect) => this.m_DraggedY = Mathf.Clamp(
            Event.current.mousePosition.y - listRect.y, this.m_DragOffset,
            listRect.height - (this.GetElementHeight(this.m_ActiveElement) - this.m_DragOffset));

        private int CalculateRowIndex() => this.GetRowIndex(this.m_DraggedY);

        private int GetRowIndex(float localY)
        {
            // if (this.elementHeightCallback == null)
            //     return Mathf.Clamp(Mathf.FloorToInt(localY / this.elementHeight), 0, this.count - 1);
            float num1 = 0.0f;
            for (int index = 0; index < this.count; ++index)
            {
                float num2 = GetElementHeight(index);
                float num3 = num1 + num2;
                if ((double) localY >= (double) num1 && (double) localY < (double) num3)
                    return index;
                num1 += num2;
            }

            return this.count - 1;
        }

        private bool IsDragging() => this.m_Dragging;
        
        

        protected virtual GUIContent GetAddIcon()
        {
            return s_Defaults.iconToolbarPlus;
        }

        public void SetExpanded(bool value)
        {
            if (value == m_Expanded)
                return;
            
            m_Expanded = value;
            RequestRepaint();
        }

        public void GrabKeyboardFocus() => GUIUtility.keyboardControl = this.id;

        public void ReleaseKeyboardFocus()
        {
            if (GUIUtility.keyboardControl != this.id)
                return;
            GUIUtility.keyboardControl = 0;
        }

        public bool HasKeyboardControl() =>
            GUIUtility.keyboardControl == this.id && CustomEditorGUI.HasCurrentWindowKeyFocus();
        
        public void RequestRepaint()
        {
            RepaintRequested?.Invoke();
        }

        public delegate void HeaderCallbackDelegate(Rect rect);

        public delegate void FooterCallbackDelegate(Rect rect);

        public delegate void ElementCallbackDelegate(
            Rect rect,
            int index,
            bool isActive,
            bool isFocused);

        public delegate float ElementHeightCallbackDelegate(int index);

        public delegate void DrawNoneElementCallback(Rect rect);

        public delegate void ReorderCallbackDelegateWithDetails(
            BetterReorderableList list,
            int oldIndex,
            int newIndex);

        public delegate void ReorderCallbackDelegate(BetterReorderableList list);

        public delegate void SelectCallbackDelegate(BetterReorderableList list);

        public delegate void AddCallbackDelegate(BetterReorderableList list);

        public delegate void AddDropdownCallbackDelegate(Rect buttonRect, BetterReorderableList list);

        public delegate void RemoveCallbackDelegate(BetterReorderableList list);

        public delegate void ChangedCallbackDelegate(BetterReorderableList list);

        public delegate bool CanRemoveCallbackDelegate(BetterReorderableList list);

        public delegate bool CanAddCallbackDelegate(BetterReorderableList list);

        public delegate void DragCallbackDelegate(BetterReorderableList list);

        public delegate int GenericDelegate();

#region Defaults
        public class Defaults
        {
            public GUIContent iconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to list");

            public GUIContent iconToolbarPlusMore = EditorGUIUtility.TrIconContent("Toolbar Plus More", "Choose to add to list");

            public GUIContent iconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from list");

            private readonly Color altColor = new Color(0.29f, 0.29f, 0.29f, 1.0f);

            public readonly GUIStyle draggingHandle = (GUIStyle) "RL DragHandle";
            public readonly GUIStyle headerBackground = (GUIStyle) "RL Header";
            private readonly GUIStyle emptyHeaderBackground = (GUIStyle) "RL Empty Header";
            public readonly GUIStyle footerBackground = (GUIStyle) "RL Footer";
            public readonly GUIStyle boxBackground = (GUIStyle) "RL Background";
            public readonly GUIStyle preButton = (GUIStyle) "RL FooterButton";
            public readonly GUIStyle elementBackground = (GUIStyle) "RL Element";
            public const int padding = 6;
            public const int dragHandleWidth = 20;
            private static GUIContent s_ListIsEmpty = EditorGUIUtility.TrTextContent("List is Empty");

            private Texture2D MakeTex(int width, int height, Color col)
            {
                Color[] pix = new Color[width*height];
 
                for(int i = 0; i < pix.Length; i++)
                    pix[i] = col;
 
                Texture2D result = new Texture2D(width, height);
                result.SetPixels(pix);
                result.Apply();
 
                return result;
            }

            
            public void DrawFooter(Rect rect, BetterReorderableList list, bool displayAdd, bool displayRemove,
                Action<int> handleRemoveElement = null)
            {
                if (!GUI.enabled || (!displayAdd && !displayRemove)) return;
                
                float num = rect.xMax - 10f;
                float x = num - 8f;
                if (displayAdd)
                    x -= 25f;
                if (displayRemove)
                    x -= 25f;
                rect = new Rect(x, rect.y, num - x, rect.height);
                Rect rect1 = new Rect(x + 4f, rect.y, 25f, 16f);
                Rect position = new Rect(num - 29f, rect.y, 25f, 16f);
                if (Event.current.type == UnityEngine.EventType.Repaint)
                    this.footerBackground.Draw(rect, false, false, false, false);
                if (displayAdd)
                {
                    using (new EditorGUI.DisabledScope(list.onCanAddCallback != null && !list.onCanAddCallback(list)))
                    {
                        if (GUI.Button(rect1, list.GetAddIcon(), this.preButton))
                        {
                            list.OnAddElement(rect1);

                            if (list.onChangedCallback != null)
                            {
                                list.onChangedCallback(list);
                            }
                        }
                    }
                }

                if (displayRemove)
                {
                    using (new EditorGUI.DisabledScope(list.SelectedIndex < 0 || list.SelectedIndex >= list.count ||
                                                       list.onCanRemoveCallback != null && !list.onCanRemoveCallback(list)))
                    {
                        if (GUI.Button(position, this.iconToolbarMinus, this.preButton))
                        {
                            handleRemoveElement?.Invoke(list.SelectedIndex);
                        
                            if (list.onChangedCallback != null)
                                list.onChangedCallback(list);
                        }
                    }
                }
            }

            public bool TryCreateElement(BetterReorderableList list, out object item, out string error)
            {
                System.Type elementType = list._hostInfo.GetReturnType().GetCollectionElementType();
                if (TryCreateElement(elementType, out item, out error))
                    return true;
                
                return false;
            }

            public static bool TryCreateElement(Type elementType, out object element)
            {
                return TryCreateElement(elementType, out element, out _);
            }

            public static bool TryCreateElement(Type elementType, out object element, out string errorString)
            {
                errorString = null;
                if (elementType == null)
                {
                    element = null;
                    errorString = "Cannot create element of type Null.";
                    return false;
                }
                
                if (elementType == typeof(string))
                {
                    element = string.Empty;
                    return true;
                }

                if (elementType.InheritsFrom<UnityEngine.Object>())
                {
                    element = null;
                    return true;
                }
                
                /*
                var hasConstructor = elementType.GetConstructor(System.Type.EmptyTypes) != null;
                
                if (!hasConstructor)
                {
                    element = null;
                    errorString = $"Cannot add element. Type '{elementType.GetNiceName()}' has no default constructor. " +
                                  $"Implement a default constructor or implement your own add behaviour.";
                    return false;
                }*/
                
                element = elementType.CreateInstance();
                return true;
            }

            public void DrawHeaderBackground(Rect headerRect)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                if ((double) headerRect.height < 5.0)
                    this.emptyHeaderBackground.Draw(headerRect, false, false, false, false);
                else
                    this.headerBackground.Draw(headerRect, false, false, false, false);
            }

            public bool DrawHeader(Rect headerRect, bool expanded, SerializedProperty property, IList elementList)
            {
                expanded = eUtility.Foldout(expanded, GUIContent.none);
                
                // var icon = expanded ? "IN_foldout_on" : "IN_foldout";
                // if (CustomEditorGUI.IconButton(UnityIcon.InternalIcon(icon)))
                //     expanded = !expanded;
                
                DrawHeader(headerRect, property, elementList);
                return expanded;
            }
            
            public void DrawHeader(Rect headerRect, SerializedProperty property, IList elementList)
            {
                EditorGUI.LabelField(headerRect, GUIContentHelper.TempContent(property != null ? property.displayName : "IList"));
            }

            public void DrawElementBackground(
                Rect rect,
                int index,
                bool selected,
                bool focused,
                bool hovering,
                bool draggable)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                
                this.elementBackground.Draw(rect, false, selected, selected, focused);

                if (selected) return;
                
                if (hovering)
                    EditorGUI.DrawRect(rect, CustomGUIStyles.HoverColor);
                else if (index % 2 == 1)
                    EditorGUI.DrawRect(rect, altColor);
                
            }

            public void DrawElementDraggingHandle(
                Rect rect,
                int index,
                bool selected,
                bool focused,
                bool draggable)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint || !draggable)
                    return;
                this.draggingHandle.Draw(new Rect(rect.x + 5f, rect.y + 8f, 10f, 6f), false, false, false, false);
            }

            public void DrawElement(
                Rect rect,
                SerializedProperty element,
                object listItem,
                bool selected,
                bool focused,
                bool draggable)
            {
                string label = string.Empty;
                if (element != null)
                    label = element.displayName;
                else if (listItem != null)
                    label = listItem.ToString();
                else
                    label = "<None>";
                EditorGUI.LabelField(rect, GUIContentHelper.TempContent(label));
            }

            public void DrawNoneElement(Rect rect, bool draggable) =>
                EditorGUI.LabelField(rect, BetterReorderableList.Defaults.s_ListIsEmpty);
        }

        protected virtual int Add(object element)
        {
            if (SerializedProperty != null)
            {
                ++SerializedProperty.arraySize;
                var newIndex = SerializedProperty.arraySize - 1;
                var elementProperty = SerializedProperty.GetArrayElementAtIndex(newIndex);
                if (element != null)
                    elementProperty.SetValue(element);
                return newIndex;
            }

            // if (m_ElementList == null)
            //     m_ElementList = (IList)Activator.CreateInstance(this.m_ListType);

            if (m_ElementList is Array)
            {
                var newIndex = List.Count;
                SetArrayElement(newIndex, element);
                return newIndex;
            }
            
            return m_ElementList.Add(element);
        }

        protected virtual void SetArrayElement(int newIndex, object element)
        {
            m_ElementList = (IList)Utility.ResizeArrayGeneric(m_ElementList, newIndex + 1);
            m_ElementList[newIndex] = element;
        }

        #endregion
    }
}