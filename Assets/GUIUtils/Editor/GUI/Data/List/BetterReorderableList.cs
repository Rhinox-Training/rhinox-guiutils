// Decompiled with JetBrains decompiler
// Type: UnityEditorInternal.ReorderableList
// Assembly: UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FDB4E7A2-29AE-4EF7-91F6-716E71E93744
// Assembly location: D:\software\UnityEditor\2019.4.30f1\Editor\Data\Managed\UnityEditor.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public BetterReorderableList.RemoveCallbackDelegate onRemoveCallback;
        public BetterReorderableList.DragCallbackDelegate onMouseDragCallback;
        public BetterReorderableList.SelectCallbackDelegate onMouseUpCallback;
        public BetterReorderableList.CanRemoveCallbackDelegate onCanRemoveCallback;
        public BetterReorderableList.CanAddCallbackDelegate onCanAddCallback;
        public BetterReorderableList.ChangedCallbackDelegate onChangedCallback;
        private int m_ActiveElement = -1;
        private float m_DragOffset = 0.0f;
        private ExposedGUISlideGroup m_SlideGroup;
        protected SerializedObject m_SerializedObject;
        protected SerializedProperty m_Elements;
        protected IList m_ElementList;
        protected Type m_ListType;
        protected Type m_ElementType;
        private bool m_Draggable;
        private float m_DraggedY;
        private bool m_Dragging;
        private List<int> m_NonDragTargetIndices;
        private bool m_DisplayHeader;
        public bool displayAdd;
        public bool displayRemove;
        private int id = -1;
        protected static BetterReorderableList.Defaults s_Defaults;
        public float elementHeight = 21f;
        public float headerHeight = 20f;
        public float footerHeight = 20f;
        public bool showDefaultBackground = true;
        private float elementMargin = 4f;
        private const float kListElementBottomPadding = 4f;

        public static BetterReorderableList.Defaults defaultBehaviours => BetterReorderableList.s_Defaults;

        public BetterReorderableList(
            IList elements,
            bool draggable = true,
            bool displayHeader = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true)
        {
            this.InitList(null, null, elements, 
                draggable, displayHeader,
                displayAddButton, displayRemoveButton);
        }

        public BetterReorderableList(
            SerializedObject serializedObject,
            SerializedProperty elements,
            bool draggable = true,
            bool displayHeader = true,
            bool displayAddButton = true,
            bool displayRemoveButton = true)
        {
            this.InitList(serializedObject, elements, null, 
                draggable, displayHeader, 
                displayAddButton, displayRemoveButton);
        }

        protected virtual void InitList(
            SerializedObject serializedObject,
            SerializedProperty elements,
            IList elementList,
            bool draggable,
            bool displayHeader,
            bool displayAddButton,
            bool displayRemoveButton)
        {
            this.id = CustomGUIUtility.GetPermanentControlID();
            this.m_SerializedObject = serializedObject;
            this.m_Elements = elements;
            this.m_ElementList = elementList;

            if (elementList != null)
            {
                this.m_ListType = elementList.GetType();
                this.m_ElementType = this.m_ListType.GetCollectionElementType();
            }
            else
            {
                System.Reflection.FieldInfo fi = elements.FindFieldInfo();
                this.m_ListType = fi.FieldType;
                this.m_ElementType = this.m_ListType.GetCollectionElementType();
            }
            
            this.m_Draggable = draggable;
            this.m_Dragging = false;
            this.m_SlideGroup = new ExposedGUISlideGroup();
            this.displayAdd = displayAddButton;
            this.m_DisplayHeader = displayHeader;
            this.displayRemove = displayRemoveButton;
            if (this.m_Elements != null && !this.m_Elements.editable)
                this.m_Draggable = false;
            if (this.m_Elements == null || this.m_Elements.isArray)
                return;
            Debug.LogError("Input elements should be an Array SerializedProperty");
        }

        public SerializedProperty serializedProperty
        {
            get => m_Elements;
            set => m_Elements = value;
        }

        public IList list
        {
            get => m_ElementList;
            set => m_ElementList = value;
        }

        public int index
        {
            get => m_ActiveElement;
            set => m_ActiveElement = value;
        }

        private float listElementTopPadding => CustomGUIUtility.Padding;

        public bool draggable
        {
            get => m_Draggable;
            set => m_Draggable = value;
        }

        private Rect GetContentRect(Rect rect)
        {
            Rect contentRect = rect;
            if (draggable)
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
                if (this.m_Elements == null)
                    return this.m_ElementList.Count;
                if (!this.m_Elements.hasMultipleDifferentValues)
                    return this.m_Elements.arraySize;
                int val2 = this.m_Elements.arraySize;
                foreach (UnityEngine.Object targetObject in this.m_Elements.serializedObject.targetObjects)
                    val2 = Math.Min(
                        new SerializedObject(targetObject).FindProperty(this.m_Elements.propertyPath).arraySize, val2);
                return val2;
            }
        }

        public void DoLayoutList()
        {
            if (BetterReorderableList.s_Defaults == null)
                BetterReorderableList.s_Defaults = new BetterReorderableList.Defaults();
            Rect rect1 = GUILayoutUtility.GetRect(0.0f, this.headerHeight, GUILayout.ExpandWidth(true));
            Rect rect2 = GUILayoutUtility.GetRect(10f, this.GetListElementHeight(), GUILayout.ExpandWidth(true));
            Rect rect3 = GUILayoutUtility.GetRect(4f, this.footerHeight, GUILayout.ExpandWidth(true));

            // if (!rect2.IsValid())
            //     return;
            
            this.DoListHeader(rect1);
            this.DoListElements(rect2);
            this.DoListFooter(rect3);
        }

        public void DoList(Rect rect)
        {
            if (BetterReorderableList.s_Defaults == null)
                BetterReorderableList.s_Defaults = new BetterReorderableList.Defaults();
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, this.headerHeight);
            Rect listRect = new Rect(rect.x, headerRect.y + headerRect.height, rect.width, this.GetListElementHeight());
            Rect footerRect = new Rect(rect.x, listRect.y + listRect.height, rect.width, this.footerHeight);
            this.DoListHeader(headerRect);
            this.DoListElements(listRect);
            this.DoListFooter(footerRect);
        }

        public float GetHeight() => 0.0f + this.GetListElementHeight() + this.headerHeight + this.footerHeight;

        private float GetListElementHeight()
        {
            float num = 4f + this.listElementTopPadding;
            int count = GetListDrawCount();
            if (count == 0)
                return this.elementHeight + num;
            return this.GetElementYOffset(count - 1) + this.GetElementHeight(count - 1) + num;
        }

        private void DoListElements(Rect listRect)
        {
            int count = GetListDrawCount();
            if (this.showDefaultBackground && Event.current.type == UnityEngine.EventType.Repaint)
                BetterReorderableList.s_Defaults.boxBackground.Draw(listRect, false, false, false, false);

            listRect.yMin += this.listElementTopPadding;
            listRect.yMax -= 4f;
            if (this.showDefaultBackground)
            {
                ++listRect.xMin;
                --listRect.xMax;
            }

            Rect rect1 = listRect;
            rect1.height = this.elementHeight;

            if ((this.m_Elements != null && this.m_Elements.isArray || this.m_ElementList != null) && count > 0)
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
                            rect1.height = this.GetElementHeight(index);
                            rect1.y = listRect.y + this.GetElementYOffset(this.m_NonDragTargetIndices[index],
                                this.m_ActiveElement);
                            if (flag)
                                rect1.y += GetElementHeight(this.m_ActiveElement);
                            rect1 = this.m_SlideGroup.GetRect(this.m_NonDragTargetIndices[index], rect1);
                            OnDrawElementBackground(rect1, index, false, false, m_Draggable);
                            
                            s_Defaults.DrawElementDraggingHandle(rect1, index, false, false, this.m_Draggable);
                            Rect contentRect = this.GetContentRect(rect1);
                            DrawElement(contentRect, this.m_NonDragTargetIndices[index]);
                        }
                        else
                            flag = true;
                    }

                    rect1.y = this.m_DraggedY - this.m_DragOffset + listRect.y;
                    OnDrawElementBackground(rect1, this.m_ActiveElement, true, true, m_Draggable);
                    s_Defaults.DrawElementDraggingHandle(rect1, this.m_ActiveElement, true, true, this.m_Draggable);
                    Rect contentRect1 = this.GetContentRect(rect1);
                    DrawElement(contentRect1, m_ActiveElement, true, true);
                }
                else
                {
                    for (int index = 0; index < count; ++index)
                    {
                        bool flag1 = index == this.m_ActiveElement;
                        bool flag2 = index == this.m_ActiveElement && this.HasKeyboardControl();
                        rect1.height = this.GetElementHeight(index);
                        rect1.y = listRect.y + this.GetElementYOffset(index);
                        OnDrawElementBackground(rect1, index, flag1, flag2, this.m_Draggable);
                        s_Defaults.DrawElementDraggingHandle(rect1, index, flag1, flag2, this.m_Draggable);
                        Rect contentRect = this.GetContentRect(rect1);

                        DrawElement(contentRect, index, flag1, flag2);
                    }
                }

                this.DoDraggingAndSelection(listRect);
            }
            else
            {
                rect1.y = listRect.y;
                OnDrawElementBackground(rect1, -1, false, false, false);
                BetterReorderableList.s_Defaults.DrawElementDraggingHandle(rect1, -1, false, false, false);
                Rect rect2 = rect1;
                rect2.xMin += 6f;
                rect2.xMax -= 6f;
                if (this.drawNoneElementCallback == null)
                    BetterReorderableList.s_Defaults.DrawNoneElement(rect2, this.m_Draggable);
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
            if (this.drawElementCallback == null)
            {
                if (this.m_Elements != null)
                    s_Defaults.DrawElement(contentRect,
                        this.m_Elements.GetArrayElementAtIndex(elementIndex),
                        (object) null, 
                        selected, focused, 
                        this.m_Draggable);
                else
                    s_Defaults.DrawElement(contentRect, 
                        (SerializedProperty) null,
                        this.m_ElementList[elementIndex], 
                        selected, focused,
                        this.m_Draggable);
            }
            else
                this.drawElementCallback(contentRect, elementIndex, selected, focused);
        }

        protected virtual void OnDrawElementBackground(Rect rect1, int index, bool selected, bool focused, bool draggable)
        {
            s_Defaults.DrawElementBackground(rect1, index, selected, focused, draggable);
        }

        private void DoListHeader(Rect headerRect)
        {
            if (this.showDefaultBackground && Event.current.type == UnityEngine.EventType.Repaint)
                BetterReorderableList.s_Defaults.DrawHeaderBackground(headerRect);
            headerRect.xMin += 6f;
            headerRect.xMax -= 6f;
            headerRect.height -= 2f;
            ++headerRect.y;
            
            OnDrawHeader(headerRect);
        }

        protected virtual void OnDrawHeader(Rect headerRect)
        {
            if (!this.m_DisplayHeader)
                return;
            s_Defaults.DrawHeader(headerRect, this.m_SerializedObject, this.m_Elements, this.m_ElementList);
        }
        
        protected virtual void OnAddElement(Rect rect1)
        {
            s_Defaults.DoAddButton(this);
        }

        protected virtual void DoListFooter(Rect footerRect)
        {
            if (!this.displayAdd && !this.displayRemove)
                return;
            s_Defaults.DrawFooter(footerRect, this, this.displayAdd, this.displayRemove);
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
                        this.m_ActiveElement = this.GetRowIndex(Event.current.mousePosition.y - listRect.y);
                        if (this.m_Draggable && Event.current.button == 0)
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
                    if (!this.m_Draggable)
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
                                if (this.m_SerializedObject != null && this.m_Elements != null)
                                {
                                    this.m_Elements.MoveArrayElement(this.m_ActiveElement, rowIndex);
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
                    if (this.m_Draggable && GUIUtility.hotControl == this.id)
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
                    if (current.keyCode == KeyCode.DownArrow)
                    {
                        ++this.m_ActiveElement;
                        current.Use();
                    }

                    if (current.keyCode == KeyCode.UpArrow)
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
                        this.m_Elements != null ? this.m_Elements.arraySize - 1 : this.m_ElementList.Count - 1);
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

        public void GrabKeyboardFocus() => GUIUtility.keyboardControl = this.id;

        public void ReleaseKeyboardFocus()
        {
            if (GUIUtility.keyboardControl != this.id)
                return;
            GUIUtility.keyboardControl = 0;
        }

        public bool HasKeyboardControl() =>
            GUIUtility.keyboardControl == this.id && CustomEditorGUI.HasCurrentWindowKeyFocus();

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

        public class Defaults
        {
            public GUIContent iconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to list");

            public GUIContent iconToolbarPlusMore =
                EditorGUIUtility.TrIconContent("Toolbar Plus More", "Choose to add to list");

            public GUIContent iconToolbarMinus =
                EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from list");

            private readonly Color altColor = new Color(0.29f, 0.29f, 0.29f, 1.0f);

            public readonly GUIStyle draggingHandle = (GUIStyle) "RL DragHandle";
            public readonly GUIStyle headerBackground = (GUIStyle) "RL Header";
            private readonly GUIStyle emptyHeaderBackground = (GUIStyle) "RL Empty Header";
            public readonly GUIStyle footerBackground = (GUIStyle) "RL Footer";
            public readonly GUIStyle boxBackground = (GUIStyle) "RL Background";
            public readonly GUIStyle preButton = (GUIStyle) "RL FooterButton";
            public readonly GUIStyle elementBackground = (GUIStyle) "RL Element";
            private GUIStyle _altElementBackground;
            public GUIStyle altElementBackground {
                get
                {
                    
                    if (_altElementBackground != null) return _altElementBackground;
                    _altElementBackground = new GUIStyle("RL Element");
                    _altElementBackground.normal.background = Utility.GetColorTexture(altColor);
                    _altElementBackground.onActive = elementBackground.onActive;
                    return _altElementBackground;
                }
            }
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

            
            public void DrawFooter(Rect rect, BetterReorderableList list, bool displayAdd, bool displayRemove)
            {
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

                if (!displayRemove)
                    return;
                using (new EditorGUI.DisabledScope(list.index < 0 || list.index >= list.count ||
                                                   list.onCanRemoveCallback != null && !list.onCanRemoveCallback(list)))
                {
                    if (GUI.Button(position, this.iconToolbarMinus, this.preButton))
                    {
                        OnRemoveElement(list);
                        
                        if (list.onChangedCallback != null)
                            list.onChangedCallback(list);
                    }
                }
            }

            public void OnRemoveElement(BetterReorderableList list)
            {
                if (list.onRemoveCallback == null)
                    this.DoRemoveButton(list);
                else
                    list.onRemoveCallback(list);
            }

            public void DoAddButton(BetterReorderableList list)
            {
                if (list.serializedProperty != null)
                {
                    ++list.serializedProperty.arraySize;
                    list.index = list.serializedProperty.arraySize - 1;
                }
                else
                {
                    System.Type elementType = list.list.GetType().GetCollectionElementType();
                    if (elementType == typeof(string))
                        list.index = list.list.Add((object) "");
                    else if (elementType != null && elementType.GetConstructor(System.Type.EmptyTypes) == null)
                        Debug.LogError((object) ("Cannot add element. Type " + elementType.ToString() +
                                                 " has no default constructor. Implement a default constructor or implement your own add behaviour."));
                    else if (elementType != null)
                        list.index = list.list.Add(Activator.CreateInstance(elementType));
                    else
                        Debug.LogError((object) "Cannot add element of type Null.");
                }
            }

            public void DoRemoveButton(BetterReorderableList list)
            {
                if (list.serializedProperty != null)
                {
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                    if (list.index < list.serializedProperty.arraySize - 1)
                        return;
                    list.index = list.serializedProperty.arraySize - 1;
                }
                else
                {
                    list.list.RemoveAt(list.index);
                    if (list.index >= list.list.Count - 1)
                        list.index = list.list.Count - 1;
                }
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

            public void DrawHeader(
                Rect headerRect,
                SerializedObject serializedObject,
                SerializedProperty element,
                IList elementList)
            {
                EditorGUI.LabelField(headerRect,
                    GUIContentHelper.TempContent(element != null ? "Serialized Property" : "IList"));
            }

            public void DrawElementBackground(
                Rect rect,
                int index,
                bool selected,
                bool focused,
                bool draggable)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                this.elementBackground.Draw(rect, false, selected, selected, focused);
            }

            public void DrawElementBackgroundAlternating(
                Rect rect,
                int index,
                bool selected,
                bool focused,
                bool draggable)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                if (index % 2 == 0)
                    s_Defaults.elementBackground.Draw(rect, false, selected, selected, focused);
                else
                    s_Defaults.altElementBackground.Draw(rect, false, selected, selected, focused);
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
                EditorGUI.LabelField(rect,
                    GUIContentHelper.TempContent(element != null ? element.displayName : listItem.ToString()));
            }

            public void DrawNoneElement(Rect rect, bool draggable) =>
                EditorGUI.LabelField(rect, BetterReorderableList.Defaults.s_ListIsEmpty);
        }
    }
}