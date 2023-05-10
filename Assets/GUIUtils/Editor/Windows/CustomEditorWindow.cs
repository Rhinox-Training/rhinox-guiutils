using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.GUIUtils.Editor
{
    public partial class CustomEditorWindow : EditorWindow, ISerializationCallbackReceiver, IRepaintable
    {
        /// <summary>
        /// Gets the label width to be used. Values between 0 and 1 are treated as percentages, and values above as pixels.
        /// </summary>
        [SerializeField, HideInInspector] private float _labelWidth = 0.33f;
        public virtual float DefaultLabelWidth
        {
            get => _labelWidth;
            set => _labelWidth = value;
        }

        /// <summary>
        /// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
        /// </summary>
        [SerializeField, HideInInspector] private RectOffset _windowPadding;
        public virtual RectOffset WindowPadding
        {
            get => _windowPadding ?? (_windowPadding = new RectOffset(4, 4, 4, 4));
            set => _windowPadding = value;
        }

        [SerializeField, HideInInspector] private bool _useScrollView = true;
        public virtual bool UseScrollView
        {
            get => _useScrollView;
            set => _useScrollView = value;
        }

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
        
        private static PropertyInfo s_materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.FlattenHierarchy);

        private static int s_inspectObjectWindowCount = 3;

        [SerializeField, HideInInspector] private Object _inspectorTargetSerialized;
        [NonSerialized] private object _inspectTargetObject;
        [SerializeField, HideInInspector] private int _wrappedAreaMaxHeight = 1000;
        [NonSerialized] private int _drawCountWarmup;
        [NonSerialized] private bool _initialized;
        private GUIStyle _marginStyle;
        private object[] _currentPaintedTargets = Array.Empty<object>();
        private IEditor[] _editors = Array.Empty<IEditor>();
        private Vector2 _currentScrollPosition;
        private int _mouseDownId;
        private EditorWindow _mouseDownWindow;
        private int _mouseDownKeyboardControl;
        private Vector2 _contentSize;
        private bool _repaintRequested;
        protected bool _preventContentFromExpanding;

        //==============================================================================================================
        // EVENTS

        public event Action OnBeginGUI;
        public event Action OnEndGUI;
        public event Action OnClose;
        
        /// <summary>
        /// Gets the target which which the window is supposed to draw.
        /// By default it simply returns the editor window instance itself.
        /// </summary>
        protected virtual object GetTarget()
        {
            if (_inspectTargetObject != null)
                return _inspectTargetObject;
            return _inspectorTargetSerialized != null
                ? _inspectorTargetSerialized
                : (object)this;
        }

        protected virtual IEnumerable<object> GetTargets()
        {
            yield return GetTarget();
        }
        
        private void SetupAutomaticHeightAdjustment(int maxHeight)
        {
            _preventContentFromExpanding = true;
            _wrappedAreaMaxHeight = maxHeight;
            int screenHeight = Screen.currentResolution.height - 40;
            Rect originalP = RoundValues(position);
            Rect currentP = originalP;
            CustomEditorWindow wnd = this;
            int getGoodOriginalCounter = 0;
            int tmpFrameCount = 0;
            EditorApplication.CallbackFunction callback = null;
            callback = () =>
            {
                EditorApplication.update -= callback;
                EditorApplication.update -= callback;
                if (wnd == null)
                    return;
                if (tmpFrameCount++ < 10)
                    wnd.Repaint();
                if (getGoodOriginalCounter <= 1 && originalP.y < 1.0)
                {
                    ++getGoodOriginalCounter;
                    originalP = position;
                }
                else
                {
                    int y = (int)_contentSize.y;
                    if (y != (int)currentP.height)
                    {
                        tmpFrameCount = 0;
                        currentP = originalP; // Copy with changed height
                        currentP.height = Math.Min(y, maxHeight);

                        wnd.minSize = new Vector2(wnd.minSize.x, currentP.height);
                        wnd.maxSize = new Vector2(wnd.maxSize.x, currentP.height);
                        if (currentP.yMax >= (double)screenHeight)
                            currentP.y -= currentP.yMax - screenHeight;
                        wnd.position = currentP;
                    }
                }

                EditorApplication.update += callback;
            };
            EditorApplication.update += callback;
        }

        protected virtual void OnGUI()
        {
            bool preventExpansion = _preventContentFromExpanding;
            if (preventExpansion)
                GUILayout.BeginArea(new Rect(0.0f, 0.0f, position.width, _wrappedAreaMaxHeight));
            
            OnBeginGUI?.Invoke();

            InitializeIfNeeded();
            if (_marginStyle == null)
                _marginStyle = new GUIStyle { padding = new RectOffset() };
            
            if (Event.current.type == EventType.Layout)
            {
                _marginStyle.padding = WindowPadding;
                UpdateEditors();
            }

            EventType type = Event.current.type;
            if (Event.current.type == EventType.MouseDown)
            {
                _mouseDownId = GUIUtility.hotControl;
                _mouseDownKeyboardControl = GUIUtility.keyboardControl;
                _mouseDownWindow = focusedWindow;
            }

            bool useScrollView = UseScrollView;
            if (useScrollView)
                _currentScrollPosition = EditorGUILayout.BeginScrollView(_currentScrollPosition);

            var rect = !_preventContentFromExpanding ? 
                EditorGUILayout.BeginVertical() : 
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            {
                if (_contentSize == Vector2.zero || Event.current.type == EventType.Repaint)
                    _contentSize = rect.size;

                using (new eUtility.HierarchyMode(false))
                {
                    float newLabelWidth = DefaultLabelWidth >= 1.0
                        ? DefaultLabelWidth
                        : _contentSize.x * DefaultLabelWidth;
                    using (new eUtility.LabelWidth(newLabelWidth))
                    {
                        OnBeginDrawEditors();
                        GUILayout.BeginVertical(_marginStyle);
                        DrawEditors();
                        GUILayout.EndVertical();
                        OnEndDrawEditors();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            
            if (useScrollView)
                EditorGUILayout.EndScrollView();
            
            OnEndGUI?.Invoke();
            
            if (Event.current.type != type)
                _mouseDownId = -2;
            if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == _mouseDownId &&
                focusedWindow == _mouseDownWindow &&
                GUIUtility.keyboardControl == _mouseDownKeyboardControl)
            {
                GUIUtility.hotControl = 0;
                DragAndDrop.activeControlID = 0;
                GUIUtility.keyboardControl = 0;
                GUI.FocusControl(null);
            }

            if (_drawCountWarmup < 10)
            {
                Repaint();
                if (Event.current.type == EventType.Repaint)
                    ++_drawCountWarmup;
            }

            if (Event.current.isMouse || 
                Event.current.type == EventType.Used ||
                _currentPaintedTargets.IsNullOrEmpty())
                Repaint();
            
            RepaintIfRequested();
            
            if (preventExpansion)
                GUILayout.EndArea();
        }

        /// <summary>
        /// Calls DrawEditor(index) for each of the currently drawing targets.
        /// </summary>
        protected virtual void DrawEditors()
        {
            for (int i = 0; i < _currentPaintedTargets.Length; ++i)
            {
                DrawEditor(i);
                DrawEditorOverlay(i);
            }
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
                Repaint();
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

        protected virtual void OnEnable()
        {
            InitializeIfNeeded();
        }

        protected virtual void OnDestroy()
        {
            if (_editors != null)
            {
                for (int i = 0; i < _editors.Length; ++i)
                {
                    if (_editors[i] != null)
                    {
                        _editors[i].Destroy();
                        _editors[i] = null;
                    }
                }
            }

            Selection.selectionChanged -= Repaint;
            
            OnClose?.Invoke();
        }

        private void InitializeIfNeeded()
        {
            if (_initialized)
                return;
            _initialized = true;
            if (titleContent != null && titleContent.text == GetType().FullName)
                titleContent.text = GetType().GetNameWithNesting().SplitCamelCase();
            wantsMouseMove = true;
            Selection.selectionChanged -= Repaint;
            Selection.selectionChanged += Repaint;
            Initialize();
        }

        /// <summary>
        /// Initialize get called by OnEnable and by OnGUI after assembly reloads
        /// which often happens when you recompile or enter and exit play mode.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        protected virtual void DrawEditorOverlay(int index)
        {
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
                    DrawEditorPreview(index, DefaultEditorPreviewHeight);

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

        protected virtual void OnBeginDrawEditors()
        {
        }

        protected virtual void OnEndDrawEditors()
        {
        }
        
        public void RequestRepaint()
        {
            _repaintRequested = true;
        }

        protected void RepaintIfRequested()
        {
            if (!_repaintRequested)
                return;
            if (this)
                Repaint();
            _repaintRequested = false;
        }
        
        private static Rect RoundValues(Rect r)
        {
            r.x = (int)r.x;
            r.y = (int)r.y;
            r.width = (int)r.width;
            r.height = (int)r.height;
            return r;
        }

        //==============================================================================================================
        // SERIALIZATION CALLBACKS
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            OnBeforeSerialize();
        }
        
        protected virtual void OnAfterDeserialize()
        {
        }

        protected virtual void OnBeforeSerialize()
        {
        }
    }
}