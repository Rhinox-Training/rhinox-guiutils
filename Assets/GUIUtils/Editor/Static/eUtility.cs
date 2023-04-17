using System;
using System.Collections.Generic;
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

        public static bool FoldoutHeader(bool foldout, string title, GUIStyle headerStyle = null)
        {
            return FoldoutHeader(foldout, GUIContentHelper.TempContent(title), headerStyle: headerStyle);
        }
        
        public static bool FoldoutHeader(bool foldout, string title, string subTitle, GUIStyle headerStyle = null)
        {
            return FoldoutHeader(foldout, GUIContentHelper.TempContent(title), subTitle, headerStyle);
        }

        public static bool FoldoutHeader(bool foldout, GUIContent titleContent, string subTitle = null, GUIStyle headerStyle = null)
        {
            if (headerStyle == null) headerStyle = EditorStyles.boldLabel;
            
            GUILayout.BeginVertical(); // start area so this entire header is grouped in 1 rect & we can fetch it with GetLastRect()
            
            GUILayout.Space(1);
            
            // reserve space & calculate rects
            var backgroundRect = GUILayoutUtility.GetRect(1, 17);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 16f;

            const float iconSize = 13f;
            var foldoutRect = new Rect(backgroundRect);
            foldoutRect.y += 1f;
            foldoutRect.width = iconSize;
            foldoutRect.height = iconSize;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;

            // Draw rects
            EditorGUI.DrawRect(backgroundRect, CustomGUIStyles.BoxBackgroundColor);
            
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
            
            foldout = GUI.Toggle(foldoutRect, foldout, GUIContent.none, EditorStyles.foldout);

            GUILayout.Space(1);

            // handle mouse
            var e = Event.current;
            if (e.type == EventType.MouseDown && IsMouseOver(backgroundRect, e) && e.button == 0)
            {
                foldout = !foldout;
                e.Use ();
            }
            
            GUILayout.EndVertical();

            return foldout;
        }

        public static void Header(GUIContent titleContent, string subTitle = null, GUIStyle headerStyle = null)
        {
            if (headerStyle == null) headerStyle = EditorStyles.boldLabel;

            GUILayout.Space(1);

            // reserve space & calculate rects
            var backgroundRect = GUILayoutUtility.GetRect(1, 17);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 16f;

            const float iconSize = 13f;
            var foldoutRect = new Rect(backgroundRect);
            foldoutRect.y += 1f;
            foldoutRect.width = iconSize;
            foldoutRect.height = iconSize;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;

            // Draw rects
            EditorGUI.DrawRect(backgroundRect, CustomGUIStyles.BoxBackgroundColor);

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