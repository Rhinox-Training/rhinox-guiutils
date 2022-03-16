using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif

namespace Rhinox.GUIUtils.Editor
{
    public static partial class eUtility
    {
        public class Box : EditorGUILayout.VerticalScope
        {
            public Box() : base("Box")
            {
                
            }
        }
        
        public class DisabledGroup : IDisposable
        {
            public DisabledGroup(bool disabled)
            {
                EditorGUI.BeginDisabledGroup(disabled);
            }

            public virtual void Dispose()
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        public class HorizontalGroup : GUI.Scope
        {
            private bool _disabled = false;
            public Rect Rect;

            public HorizontalGroup() : this(false, GUIStyle.none, Array.Empty<GUILayoutOption>())
            {
            }
            
            public HorizontalGroup(GUIStyle style) : this(false, style, Array.Empty<GUILayoutOption>())
            {
            }
            
            public HorizontalGroup(params GUILayoutOption[] options) : this(false, GUIStyle.none, options)
            {
            }
            
            public HorizontalGroup(bool disabled, params GUILayoutOption[] options) : this(disabled, GUIStyle.none, options)
            {
            }

            public HorizontalGroup(bool disabled, GUIStyle style, params GUILayoutOption[] options)
            {
                _disabled = disabled;
                Rect = EditorGUILayout.BeginHorizontal(style, options);

                if (_disabled)
                    EditorGUI.BeginDisabledGroup(disabled);
            }

            protected override void CloseScope()
            {
                if (_disabled)
                    EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
        }

        public class VerticalGroup : GUI.Scope
        {
            private bool _disabled = false;
            public Rect Rect;

            public VerticalGroup() : this(false, GUIStyle.none, Array.Empty<GUILayoutOption>())
            {
            }
            
            public VerticalGroup(GUIStyle style) : this(false, style, Array.Empty<GUILayoutOption>())
            {
            }
            
            public VerticalGroup(params GUILayoutOption[] options) : this(false, GUIStyle.none, options)
            {
            }
            
            public VerticalGroup(bool disabled, params GUILayoutOption[] options) : this(disabled, GUIStyle.none, options)
            {
            }

            public VerticalGroup(bool disabled, GUIStyle style, params GUILayoutOption[] options)
            {
                _disabled = disabled;
                Rect = EditorGUILayout.BeginVertical(style, options);

                if (_disabled)
                    EditorGUI.BeginDisabledGroup(disabled);
            }

            protected override void CloseScope()
            {
                if (_disabled)
                    EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();
            }
        }
        
        public class GuiColor : IDisposable
        {
            private Color _prevColor;

            public GuiColor(Color color, bool blendAlpha = false)
            {
                _prevColor = GUI.color;
                if (blendAlpha)
                    color.a *= GUI.color.a;
                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = _prevColor;
            }
        }
        
        public class HandleColor : IDisposable
        {
            private Color _prevColor;
            public HandleColor(Color color)
            {
                _prevColor = Handles.color;
                Handles.color = color;
            }

            public void Dispose()
            {
                Handles.color = _prevColor;
            }
        }
        
        public class GizmoColor : IDisposable
        {
            private Color _prevColor;
            public GizmoColor(Color color)
            {
                _prevColor = Gizmos.color;
                Gizmos.color = color;
            }
            
            public GizmoColor(float r, float g, float b, float a = 1f)
            {
                _prevColor = Gizmos.color;
                Gizmos.color = new Color(r, g, b, a);
            }

            public void Dispose()
            {
                Gizmos.color = _prevColor;
            }
        }

        public class Rotation : IDisposable
        {
            public Matrix4x4 _originalMatrix;

            public Rotation(float angle, Vector2 pivot)
            {
                _originalMatrix = GUI.matrix;
                if (Math.Abs(angle) < float.Epsilon) return;
                GUIUtility.RotateAroundPivot(angle, pivot);
            }

            // public Rotation(float angle, float widthOfTargetRect, float heightOfTargetRect)
            // {
            //     _originalMatrix = GUI.matrix;
            //     var pivot = GUIHelper.EditorScreenPointOffset;
            //     pivot += new Vector2(widthOfTargetRect/2, heightOfTargetRect/2);
            //     GUIUtility.RotateAroundPivot(angle, pivot);
            // }

            public void Dispose()
            {
                GUI.matrix = _originalMatrix;
            }
        }
        
        public class FoldoutContainer : GUI.Scope
        {
            private static readonly GUIStyle DefaultContainerStyle;
            private static readonly GUIStyle DefaultLabelStyle;

            static FoldoutContainer ()
            {
                DefaultContainerStyle = GUI.skin.FindStyle ("Box");
                DefaultLabelStyle = new GUIStyle (EditorStyles.foldout);
            }

            public bool isOpen { get; private set; }

            public FoldoutContainer (ref bool isOpen, string text) : this (ref isOpen, text, DefaultContainerStyle, DefaultLabelStyle) { }
            public FoldoutContainer (ref bool isOpen, string text, GUIStyle containerStyle, GUIStyle labelStyle)
            {
                this.isOpen = isOpen;
                GUIContentHelper.PushIndentLevel(1);
                
                EditorGUILayout.BeginVertical(containerStyle);
                GUILayout.Space (3);
                
                isOpen = EditorGUI.Foldout (EditorGUILayout.GetControlRect(), isOpen, text, true, labelStyle);
            }

            public FoldoutContainer (SerializedProperty isExpanded, string text) : this (isExpanded, text, DefaultContainerStyle, DefaultLabelStyle) { }
            public FoldoutContainer (SerializedProperty isExpanded, string text, GUIStyle containerStyle, GUIStyle labelStyle)
            {
                this.isOpen = isExpanded.isExpanded;
                GUIContentHelper.PushIndentLevel(1);
                
                EditorGUILayout.BeginVertical(containerStyle);
                GUILayout.Space (3);
                
                using (var check = new EditorGUI.ChangeCheckScope ())
                {
                    isOpen = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), isOpen, text, true, labelStyle);
                    if (!check.changed) return;
                    
                    isExpanded.serializedObject.ApplyModifiedPropertiesWithoutUndo ();
                    isExpanded.isExpanded = isOpen;
                }
            }

            protected override void CloseScope()
            {
                GUILayout.Space(3);
                EditorGUILayout.EndVertical();
                
                GUIContentHelper.PopIndentLevel();
            }
        }
  
#if ODIN_INSPECTOR      
        public class FoldoutData
        {
            public float Time;
            public bool IsExpanded;
            
            public FoldoutData() : this(SirenixEditorGUI.ExpandFoldoutByDefault) {}

            public FoldoutData(bool startsExpanded)
            {
                IsExpanded = startsExpanded;
            }

            public void UpdateTime()
            {
                // assuming event type == EventType.Layout
                EditorTimeHelper.Time.Update();
                Time = Mathf.MoveTowards(Time, IsExpanded ? 1f : 0.0f, EditorTimeHelper.Time.DeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
            }
            
            public static implicit operator bool(FoldoutData d) => d.IsExpanded;
            public static implicit operator float(FoldoutData d) => d.Time;

            public void Draw(string name, Action drawFunc, string subTitle = null, int indentLevel = 1)
            {
                IsExpanded = FoldoutHeader(IsExpanded, name, subTitle);
            
                if (Event.current.type == EventType.Layout)
                    UpdateTime();

                if (!IsExpanded && Time <= 0)
                    return;

                using (new FadeGroup(Time, indentLevel))
                    drawFunc?.Invoke();
            }
        }

        public class FadeGroup : IDisposable
        {
            public FadeGroup(float t, int indentLevel = 1)
            {
                SirenixEditorGUI.BeginFadeGroup(t);
                GUIHelper.PushIndentLevel(indentLevel);
            }
            
            public void Dispose()
            {
                GUIHelper.PopIndentLevel();
                SirenixEditorGUI.EndFadeGroup();
            }
        }
#endif
        
        /// <summary>
        /// WIP, not working.
        /// </summary>
        public class PreserveSelection : IDisposable
        {
            private readonly bool _wasSelected;

            private readonly string _control;
            private int _selectionStart;
            private int _selectionEnd;
            private int _textLength;
            
            private TextEditor _editor;
            
            public PreserveSelection(string controlName)
            {
                _control = controlName;
                _wasSelected = IsSelected();
                if (!_wasSelected) return;

                var editor = GetTextEditor();

                if (editor == null) return;
                
                _selectionStart = editor.selectIndex;
                _selectionEnd = editor.cursorIndex;
                _textLength = editor.text.Length;
            }

            public void Dispose()
            {
                if (!_wasSelected || IsSelected()) return;

                GUI.FocusControl(_control);
                var editor = GetTextEditor();
                
                if (editor == null) return;

                var diff = editor.text.Length - _textLength;
                editor.cursorIndex = _selectionEnd - diff;
            }

            private bool IsSelected() => GUI.GetNameOfFocusedControl() == _control;

            private static FieldInfo _recycledTextEditorField;
            private TextEditor GetTextEditor()
            {
                TextEditor editor = null;
                try {
                    editor = GUIUtility.QueryStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
                }
                catch { }

                if (editor == null)
                {
                    if (_recycledTextEditorField == null)
                        _recycledTextEditorField = typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    editor = (TextEditor) _recycledTextEditorField.GetValue(null);
                }
                return editor;
            }
        }
    }
}