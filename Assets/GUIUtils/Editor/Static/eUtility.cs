using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
#if ODIN_INSPECTOR
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public static partial class eUtility
    {
        private static Assembly _editorAssembly;
        public static Assembly EditorAssembly => _editorAssembly ?? (_editorAssembly = Assembly.GetAssembly(typeof(EditorWindow)));
        
        /// ================================================================================================================
        /// ELEMENTS
        public static void Card(Action draw, Action onClick = null, Action<Rect> postDraw = null, float alpha = 0f)
        {
            // GUIHelper.RequestRepaint();
            var rect = EditorGUILayout.BeginVertical();

            if (Math.Abs(alpha) < float.Epsilon)
            {
                bool isMouseOver = IsMouseOver(rect);
                alpha = (EditorGUIUtility.isProSkin ? 0.25f : 0.45f) * (isMouseOver ? 2 : 1);
            }
        
            GUIContentHelper.PushColor(new Color(1, 1, 1, alpha));
            GUILayout.BeginHorizontal(CustomGUIStyles.Card);
            GUIContentHelper.PopColor();
            {
                GUILayout.BeginVertical();

                draw();
            
                GUILayout.EndVertical();
                
                postDraw?.Invoke(rect);
            
                if (onClick != null && GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    onClick.Invoke();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        public static bool Foldout(bool foldout, string label, GUIStyle style = null)
            => Foldout(foldout, GUIContentHelper.TempContent(label), style);
        
        public static bool Foldout(bool foldout, string label, out Rect contentRect, GUIStyle style = null)
            => Foldout(foldout, GUIContentHelper.TempContent(label), out contentRect, style);
        
        public static bool Foldout(Rect rect, bool foldout, string label, GUIStyle style = null)
            => Foldout(rect, foldout, GUIContentHelper.TempContent(label), style);
        
        public static bool Foldout(Rect rect, bool foldout, string label, out Rect contentRect, GUIStyle style = null)
            => Foldout(rect, foldout, GUIContentHelper.TempContent(label), out contentRect, style);
        
        public static bool Foldout(bool foldout, GUIContent label, GUIStyle style = null)
            => EditorGUILayout.Foldout(foldout, label, true, style ?? EditorStyles.foldout);
        
        public static bool Foldout(bool foldout, GUIContent label, out Rect contentRect, GUIStyle style = null)
        {
            var rect = GUILayoutUtility.GetRect(label, style);
            return Foldout(rect, foldout, label, out contentRect, style ?? EditorStyles.foldout);
        }
        
        public static bool Foldout(Rect rect, bool foldout, GUIContent label, out Rect contentRect, GUIStyle style = null)
        {
            rect.SplitX(EditorGUIUtility.labelWidth + CustomGUIUtility.Padding, out Rect foldoutRect, out contentRect);
            return Foldout(foldoutRect, foldout, label, style);
        }

        public static bool Foldout(Rect rect, bool foldout, GUIContent label, GUIStyle style = null)
            => EditorGUI.Foldout(rect, foldout, label, true, style ?? EditorStyles.foldout);
        

        public static bool FoldoutHeader(Rect rect, bool foldout, GUIContent titleContent, out Rect contentRect, GUIStyle boxStyle = null)
        {
            boxStyle?.Draw(rect, false, false, false, false);
            return Foldout(rect, foldout, titleContent, out contentRect, CustomGUIStyles.BoldFoldout);
        }

        public static bool FoldoutHeader(bool foldout, string title)
            => FoldoutHeader(foldout, GUIContentHelper.TempContent(title));

        public static bool FoldoutHeader(bool foldout, string title, string subTitle)
            => FoldoutHeader(foldout, GUIContentHelper.TempContent(title), subTitle);

        public static bool FoldoutHeader(bool foldout, GUIContent titleContent, string subTitle = null)
        {
            var style = CustomGUIStyles.BoldFoldout;
            
            GUILayout.BeginVertical(CustomGUIStyles.Clean); // start area so this entire header is grouped in 1 rect & we can fetch it with GetLastRect()
            
            GUILayout.Space(1);
            var rect = GUILayoutUtility.GetRect(titleContent, style);

            // Draw rects
            EditorGUI.DrawRect(rect, CustomGUIStyles.BoxBackgroundColor);
            
            foldout = Foldout(rect, foldout, titleContent, style);
            
            if (!string.IsNullOrWhiteSpace(subTitle))
            {
                var subHeaderStyle = CustomGUIStyles.SubtitleRight;

                var labelSize = style.CalcSize(titleContent).x;

                var remaining = rect.width - labelSize;
                rect.y += 3;
                
                titleContent = GUIContentHelper.TempContent(subTitle);
                if (remaining > subHeaderStyle.CalcSize(titleContent).x)
                    EditorGUI.LabelField(rect, titleContent, CustomGUIStyles.SubtitleRight);
            }
            
            GUILayout.Space(1);
            
            GUILayout.EndVertical();

            return foldout;
        }
        
        public static bool FoldoutHeader(bool foldout, GUIContent titleContent, out Rect contentRect, GUIStyle boxStyle = null)
        {
            GUILayout.BeginVertical(boxStyle ?? CustomGUIStyles.Clean); // start area so this entire header is grouped in 1 rect & we can fetch it with GetLastRect()
            GUILayout.Space(1);

            foldout = Foldout(foldout, titleContent, out contentRect, CustomGUIStyles.BoldFoldout);
            
            GUILayout.Space(1);
            GUILayout.EndVertical();

            return foldout;
        }
        
        public static void Header(Rect rect, GUIContent titleContent, string subTitle = null, GUIStyle headerStyle = null)
        {
            if (headerStyle == null) headerStyle = EditorStyles.boldLabel;

            var labelRect = rect;
            labelRect.xMin += 16f;
            labelRect.xMax -= CustomGUIUtility.Padding;

            // Background rect should be full-width
            rect.xMin = 0f;

            // Draw rects
            EditorGUI.DrawRect(rect, CustomGUIStyles.BoxBackgroundColor);

            EditorGUI.LabelField(labelRect, titleContent, headerStyle);

            if (!string.IsNullOrWhiteSpace(subTitle))
            {
                var subHeaderStyle = CustomGUIStyles.SubtitleRight;

                const int extraSpace = 13;
                var remaining = extraSpace + labelRect.width - headerStyle.CalcSize(titleContent).x;

                labelRect.x += extraSpace;
                labelRect.y += 3;

                titleContent = GUIContentHelper.TempContent(subTitle);
                if (remaining > subHeaderStyle.CalcSize(titleContent).x)
                    EditorGUI.LabelField(labelRect, titleContent, CustomGUIStyles.SubtitleRight);
            }
        }

        public static void Header(GUIContent titleContent, string subTitle = null, GUIStyle headerStyle = null)
        {
            GUILayout.Space(1);

            // reserve space & calculate rects
            var backgroundRect = GUILayoutUtility.GetRect(1, 17);

            Header(backgroundRect, titleContent, subTitle, headerStyle);

            GUILayout.Space(1);
        }


        public static void Header(Rect rect, GUIContent titleContent, out Rect contentRect, GUIStyle headerStyle = null)
        {
            if (headerStyle == null) headerStyle = EditorStyles.boldLabel;

            // Draw rects
            EditorGUI.DrawRect(rect, CustomGUIStyles.BoxBackgroundColor);

            rect.SplitX(EditorGUIUtility.labelWidth + CustomGUIUtility.Padding, out Rect labelRect, out contentRect);
            labelRect.xMax -= CustomGUIUtility.Padding;

            // labelRect.xMin += 16f;
            // labelRect.xMax -= 16f;
            
            EditorGUI.LabelField(labelRect, titleContent, headerStyle);
        }

        public static void Header(GUIContent titleContent, out Rect contentRect, GUIStyle headerStyle = null)
        {
            GUILayout.Space(1);

            // reserve space & calculate rects
            var backgroundRect = GUILayoutUtility.GetRect(1, 17);

            Header(backgroundRect, titleContent, out contentRect, headerStyle);

            GUILayout.Space(1);
        }

        /// ================================================================================================================
        /// UTILITY
        public static bool IsMouseOver(Rect? rect = null, Event e = null)
        {
            if (e == null) e = Event.current;
            if (e.type == EventType.Repaint) // polling this during a repaint can cause issues
                return false;

            if (rect == null) // if no rect is given, get last drawn rect (will work in case of Buttons, etc; but not in the case of Groups)
                rect = GUILayoutUtility.GetLastRect();

            return rect.Value.Contains(e.mousePosition);
        }
        
        public static bool IsClicked(ref bool isClicked, Rect? rect = null, Event e = null)
        {
            if (e == null) e = Event.current;

            // We keep track of whether the rect was clicked in mouse event, and return that value during layout
            // Otherwise doing certain operations might go wrong
            // This essentially treats a rect as if it were a button
            if (e.type == EventType.MouseDown && IsMouseOver(rect, e))
            {
                isClicked = true;
                return false;
            }

            if (isClicked && e.type == EventType.Layout) // polling this during a repaint can cause issues
                return true;
            
            if (e.type == EventType.Repaint)
                isClicked = false;

            return false;
        }

        public static bool DropZone(Func<Object[], bool> dragHandler, Rect? rect = null)
        {
            if (rect == null) rect = GUILayoutUtility.GetLastRect();

            EventType type = Event.current.type;
            switch (type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Value.Contains(Event.current.mousePosition))
                        break;

                    var draggedObjects = DragAndDrop.objectReferences;

                    if (dragHandler == null || !dragHandler(draggedObjects))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                        Event.current.Use();
                        return false;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    Event.current.Use();
                    if (type != EventType.DragPerform)
                        return false;

                    DragAndDrop.AcceptDrag();
                    return true;
            }

            return false;
        }

        public static bool DropZone(Type t, out Object[] items, Rect? rect = null, Func<Object[], bool> dragValidator = null)
        {
            if (rect == null)
                rect = GUILayoutUtility.GetLastRect();
            
            items = Array.Empty<Object>();

            EventType type = Event.current.type;
            if (!type.EqualsOneOf(EventType.DragUpdated, EventType.DragPerform))
                return false;
            
            if (!rect.Value.Contains(Event.current.mousePosition))
                return false;

            var draggedObjects = DragAndDrop.objectReferences;

            if (dragValidator == null)
            {
                foreach (var o in draggedObjects)
                {
                    if (o.GetType().InheritsFrom(t))
                        continue;
                        
                    RejectDrag();
                    return false;
                }
            }
            else if (!dragValidator(draggedObjects))
            {
                RejectDrag();
                return false;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();

            if (type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                items = draggedObjects;
                return true;
            }

            return false;
        }

        public static bool DropZone<T>(out T[] items, Rect? rect = null, Func<Object[], bool> dragValidator = null)
            where T : Object
        {
            if (DropZone(typeof(T), out Object[] objects, rect, dragValidator))
            {
                items = objects.OfType<T>().ToArray();
                return true;
            } 
            items = Array.Empty<T>();
            return false;
        }

        private static void RejectDrag()
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            Event.current.Use();
        }

        public static List<string> LayerMaskToLayers(int mask, ref List<string> listToFill)
        {
            string[] names = InternalEditorUtility.layers;

            if (listToFill == null)
                listToFill = new List<string>();
            else
                listToFill.Clear();

            for (int i = 0; i < names.Length; i++)
            {
                if (((1 << i) & mask) > 0)
                    listToFill.Add(names[i]);
            }

            return listToFill;
        }

        public static List<string> TaskMaskToTags(int tagMask, ref List<string> listToFill)
        {
            string[] tagNames = InternalEditorUtility.tags;

            if (listToFill == null)
                listToFill = new List<string>();
            else
                listToFill.Clear();

            if (tagMask == 0)
            {
                // no tags (aka 0) is still a tag: 'Untagged'; hence add 0th element
                listToFill.Add(tagNames[0]);
                return listToFill;
            }

            for (int i = 0; i < tagNames.Length; i++)
            {
                if (((1 << i) & tagMask) > 0)
                    listToFill.Add(tagNames[i]);
            }

            return listToFill;
        }
    }
}