using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class MultiTargetEditor<T> : BaseEditor<T>
        where T : class
    {
        [SerializeField, HideInInspector] private bool _drawUnityEditorPreview;
        public virtual bool DrawUnityEditorPreview
        {
            get => _drawUnityEditorPreview;
            set => _drawUnityEditorPreview = value;
        }
        
        private float _defaultEditorPreviewHeight = 170f;
        public virtual float DefaultEditorPreviewHeight
        {
            get => _defaultEditorPreviewHeight;
            set => _defaultEditorPreviewHeight = value;
        }
        
        [SerializeField, HideInInspector] private bool _useScrollView = true;
        public virtual bool UseScrollView
        {
            get => _useScrollView;
            set => _useScrollView = value;
        }
        
        private IEditor[] _editors = Array.Empty<IEditor>();
        protected Vector2 _currentScrollPosition;
        
        private object[] _currentPaintedTargets = Array.Empty<object>();
        private bool _initialized;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            InitializeIfNeeded();
        }
        
        private void InitializeIfNeeded()
        {
            if (_initialized)
                return;
            _initialized = true;
            Selection.selectionChanged -= RequestRepaint;
            Selection.selectionChanged += RequestRepaint;
            Initialize();
        }
        
        /// <summary>
        /// Initialize get called by OnEnable and by OnGUI after assembly reloads
        /// which often happens when you recompile or enter and exit play mode.
        /// </summary>
        protected virtual void Initialize()
        {
        }
        
        /// <summary>
        /// Gets the target which which the window is supposed to draw.
        /// By default it simply returns the editor window instance itself.
        /// </summary>
        protected virtual object GetTarget()
        {
            return target;
        }

        protected virtual IEnumerable<object> GetTargets()
        {
            yield return GetTarget();
        }
        
        public override void OnInspectorGUI()
        {
            InitializeIfNeeded();
            
            if (Event.current.type == EventType.Layout)
                UpdateEditors();

            bool useScrollView = UseScrollView;
            if (useScrollView)
                _currentScrollPosition = EditorGUILayout.BeginScrollView(_currentScrollPosition, CustomGUIStyles.Clean);

            using (new eUtility.HierarchyMode(false))
            {
                GUILayout.BeginVertical(CustomGUIStyles.Clean);
                DrawEditors();
                GUILayout.EndVertical();
            }
            
            if (useScrollView)
                EditorGUILayout.EndScrollView();
            
            RepaintIfRequested();
        }
        
        private void UpdateEditors()
        {
            _currentPaintedTargets = _currentPaintedTargets ?? Array.Empty<object>();
            _editors = _editors ?? Array.Empty<IEditor>();
            IList<object> targetList = GetTargets().ToArray();
            if (_currentPaintedTargets.Length != targetList.Count)
            {
                if (_editors.Length > targetList.Count)
                {
                    int num = _editors.Length - targetList.Count;
                    for (int i = 0; i < num; ++i)
                    {
                        IEditor editor = _editors[_editors.Length - i - 1];
                        editor?.Destroy();
                    }
                }

                Array.Resize(ref _currentPaintedTargets, targetList.Count);
                Array.Resize(ref _editors, targetList.Count);
                RequestRepaint();
            }

            for (int index = 0; index < targetList.Count; ++index)
            {
                object obj = targetList[index];
                object currentTarget = _currentPaintedTargets[index];
                if (obj != currentTarget) // Has target at index changed?
                {
                    RequestRepaint();
                    _currentPaintedTargets[index] = obj;
                    
                    // Refresh editor
                    if (_editors[index] != null)
                        _editors[index].Destroy();
                    
                    // Create new editor
                    _editors[index] = EditorCreator.CreateEditorForTarget(obj);
                }
            }
        }
        
        protected virtual void DrawEditors()
        {
            for (int i = 0; i < _currentPaintedTargets.Length; ++i)
            {
                DrawEditor(i);
                DrawEditorOverlay(i);
            }
        }
        
        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            try
            {
                IEditor editor = _editors[index];
                if (editor != null && editor.CanDraw())
                {
                    if (editor is IRepaintRequestHandler handler)
                        handler.UpdateRequestTarget(this);
                    editor.Draw();
                }

                if (DrawUnityEditorPreview)
                    DrawEditorPreview(index, _defaultEditorPreviewHeight);
            }
            catch (ExitGUIException)
            {
                // Rethrow, this is supported behaviour
                throw;
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox(e.ToString(), MessageType.Error);
                Debug.LogException(e);
            }
        }
        
        /// <summary>
        /// Uses the <see cref="M:UnityEditor.Editor.DrawPreview(UnityEngine.Rect)" /> method to draw a preview for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditorPreview(int index, float height)
        {
            IEditor editor = _editors[index];
            if (editor == null || !editor.HasPreviewGUI())
                return;
            Rect controlRect = EditorGUILayout.GetControlRect(false, height);
            editor.DrawPreview(controlRect);
        }
        
        protected virtual void DrawEditorOverlay(int index)
        {
        }
    }
}