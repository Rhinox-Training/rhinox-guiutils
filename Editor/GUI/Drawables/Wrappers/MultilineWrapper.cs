using System;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class MultilineWrapper : BaseWrapperDrawable
    {
        private int _minLines;
        private int MinLines => Math.Max(1, _minLines);
        private int _maxLines;
        private Vector2 _scrollPosition;
        private Rect _cachedRect;
        private int MaxLines => Math.Max(MinLines, _maxLines);

        private int ViewableLines
        {
            get
            {
                return Math.Max(MinLines, Math.Min(MaxLines, GetLineCountOfValue()));
            }
        }

        public override float ElementHeight 
        {
            get
            {
                return EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding + ViewableLines * EditorGUIUtility.singleLineHeight;
            }
        }

        public MultilineWrapper(IOrderedDrawable drawable) : base(drawable)
        {

        }

        protected override void DrawInner(GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            EditorGUILayout.LabelField(label);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(ViewableLines * EditorGUIUtility.singleLineHeight));
            var oldText = GetValue() as string;
            string newText = EditorGUILayout.TextArea(oldText, GUILayout.MaxHeight(ViewableLines * EditorGUIUtility.singleLineHeight));
            if (!string.Equals(oldText, newText, StringComparison.InvariantCulture))
                SetValue(newText);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        protected override void DrawInner(Rect rect, GUIContent label)
        {
            if (rect.IsValid())
                _cachedRect = rect;
            
            EditorGUI.LabelField(_cachedRect.AlignTop(EditorGUIUtility.singleLineHeight), label);
            var areaRect = _cachedRect.MoveDownLine(1).SetHeight(ViewableLines * EditorGUIUtility.singleLineHeight);
            
            GUILayout.BeginArea(areaRect, CustomGUIStyles.Clean);
            {
                
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                var oldText = GetValue() as string;
                string newText = EditorGUILayout.TextArea(oldText);
                if (!string.Equals(oldText, newText, StringComparison.InvariantCulture))
                    SetValue(newText);
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
            
            if (!_cachedRect.IsValid() && Event.current.type == EventType.Layout)
                RequestRepaint();
        }

        private int GetLineCountOfValue()
        {
            string data = GetValue() as string ?? string.Empty;
            return data.CountAnySubstring(new[] {"\r\n", "\r", "\n"}) + 1;
        }

        [WrapDrawer(typeof(MultilineAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(MultilineAttribute attr, IOrderedDrawable drawable)
        {
            if (drawable.HostInfo.GetReturnType() != typeof(string))
                return null;
            return new MultilineWrapper(drawable)
            {
                _minLines = attr.lines,
                _maxLines = attr.lines
            };
        }

        [WrapDrawer(typeof(TextAreaAttribute), Priority.BehaviourChange)]
        public static BaseWrapperDrawable Create(TextAreaAttribute attr, IOrderedDrawable drawable)
        {
            if (drawable.HostInfo.GetReturnType() != typeof(string))
                return null;
            return new MultilineWrapper(drawable)
            {
                _minLines = attr.minLines,
                _maxLines = attr.maxLines
            };
        }
    }
}