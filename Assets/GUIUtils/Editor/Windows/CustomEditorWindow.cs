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
    public class CustomEditorWindow : EditorWindow, ISerializationCallbackReceiver
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
        [SerializeField, HideInInspector] private Vector4 _windowPadding = new Vector4(4f, 4f, 4f, 4f);
        public virtual Vector4 WindowPadding
        {
            get => _windowPadding;
            set => _windowPadding = value;
        }

        [SerializeField, HideInInspector] private bool _useScrollView = true;
        public virtual bool UseScrollView
        {
            get => _useScrollView;
            set => _useScrollView = true;
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
        private object[] _currentPaintedTargets = new object[0];
        private ReadOnlyCollection<object> _currentTargetsImm;
        private UnityEditor.Editor[] _editors = new UnityEditor.Editor[0];
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

        /// <summary>
        /// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
        /// </summary>
        protected IReadOnlyList<object> CurrentDrawingTargets => _currentTargetsImm;

        /// <summary>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, float windowWidth)
        {
            return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0.0f));
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

        /// <summary>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, Vector2 windowSize)
        {
            CustomEditorWindow window = CreateCustomEditorWindowInstanceForObject(obj);
            if (windowSize.x <= 1.0)
                windowSize.x = btnRect.width;
            if (windowSize.x <= 1.0)
                windowSize.x = 400f;
            btnRect = RoundValues(btnRect);
            windowSize.x = (int)windowSize.x;
            windowSize.y = (int)windowSize.y;
            try
            {
                EditorWindow curr = CustomEditorGUI.CurrentWindow();
                if (curr != null)
                    window.OnBeginGUI += () => curr.Repaint();
            }
            catch
            {
            }

            window.OnEndGUI += () =>
            {
                Rect position = window.position;
                double width = position.width;
                position = window.position;
                double height = position.height;
                CustomEditorGUI.DrawBorders(new Rect(0.0f, 0.0f, (float)width, (float)height), 1);
            };
            window._labelWidth = 0.33f;
            window.DrawUnityEditorPreview = true;
            btnRect.position = GUIUtility.GUIToScreenPoint(btnRect.position);
            if ((int)windowSize.y == 0)
            {
                window.ShowAsDropDown(btnRect, new Vector2(windowSize.x, 10f));
                window.SetupAutomaticHeightAdjustment(600);
            }
            else
                window.ShowAsDropDown(btnRect, windowSize);

            return window;
        }

        /// <summary>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, Vector2 position)
        {
            Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
            return InspectObjectInDropDown(obj, btnRect, 350f);
        }

        /// <summary>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, float windowWidth)
        {
            Vector2 mousePosition = Event.current.mousePosition;
            Rect btnRect = new Rect(mousePosition.x, mousePosition.y, 1f, 1f);
            return InspectObjectInDropDown(obj, btnRect, windowWidth);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, Vector2 position, float windowWidth)
        {
            Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
            return InspectObjectInDropDown(obj, btnRect, windowWidth);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, float width, float height)
        {
            Rect btnRect = new Rect(Event.current.mousePosition, Vector2.one);
            return InspectObjectInDropDown(obj, btnRect, new Vector2(width, height));
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj)
            => InspectObjectInDropDown(obj, Event.current.mousePosition);

        /// <summary>Pops up an editor window for the given object.</summary>
        public static CustomEditorWindow InspectObject(object obj)
        {
            CustomEditorWindow instanceForObject = CreateCustomEditorWindowInstanceForObject(obj);
            instanceForObject.Show();
            Vector2 move = new Vector2(30f, 30f) * (s_inspectObjectWindowCount++ % 6 - 3);
            var baseRect = RectExtensions.AlignCenter(CustomEditorGUI.GetEditorWindowRect(), 400f, 300f);
            baseRect.position += move;
            instanceForObject.position = baseRect;
            return instanceForObject;
        }

        /// <summary>
        /// Inspects the object using an existing CustomEditorWindow.
        /// </summary>
        public static CustomEditorWindow InspectObject(CustomEditorWindow window, object obj)
        {
            Object unityObj = obj as Object;
            if (unityObj)
            {
                window._inspectTargetObject = null;
                window._inspectorTargetSerialized = unityObj;
            }
            else
            {
                window._inspectTargetObject = obj;
                window._inspectorTargetSerialized = null;
            }

            window.titleContent = GetObjectName(obj);
            EditorUtility.SetDirty(window);
            return window;
        }

        private static GUIContent GetObjectName(object obj)
        {
            if (obj is Component component)
                return new GUIContent(component.gameObject.name);
            else if (obj is UnityEngine.Object unityObj)
                return new GUIContent(unityObj.name);
            else
                return new GUIContent(obj.ToString());
        }

        /// <summary>
        /// Creates an editor window instance for the specified object, without opening the window.
        /// </summary>
        public static CustomEditorWindow CreateCustomEditorWindowInstanceForObject(object obj)
        {
            CustomEditorWindow instance = CreateInstance<CustomEditorWindow>();
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
            Object @object = obj as Object;
            if ((bool)@object)
                instance._inspectorTargetSerialized = @object;
            else
                instance._inspectTargetObject = obj;
            
            instance.titleContent = GetObjectName(obj);
            instance.position = RectExtensions.AlignCenter(CustomEditorGUI.GetEditorWindowRect(), 600f, 600f);
            EditorUtility.SetDirty(instance);
            return instance;
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
                _marginStyle.padding.left = (int)WindowPadding.x;
                _marginStyle.padding.right = (int)WindowPadding.y;
                _marginStyle.padding.top = (int)WindowPadding.z;
                _marginStyle.padding.bottom = (int)WindowPadding.w;
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
                DrawEditor(i);
        }

        private void UpdateEditors()
        {
            _currentPaintedTargets = _currentPaintedTargets ?? Array.Empty<object>();
            _editors = _editors ?? Array.Empty<UnityEditor.Editor>();
            IList<object> targetList = GetTargets().ToArray();
            if (_currentPaintedTargets.Length != targetList.Count)
            {
                if (_editors.Length > targetList.Count)
                {
                    int num = _editors.Length - targetList.Count;
                    for (int i = 0; i < num; ++i)
                    {
                        UnityEditor.Editor editor = _editors[_editors.Length - i - 1];
                        if (editor)
                            DestroyImmediate(editor);
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
                    if (_editors[index])
                        DestroyImmediate(_editors[index]);
                    
                    // Create new editor
                    UnityEditor.Editor curEditor = null;
                    if (obj is EditorWindow editorWindow)
                    {
                        curEditor = TryCreateGenericEditor(editorWindow);
                    }
                    else if (obj is UnityEngine.Object targetObject)
                    {
                        curEditor = CreateStandardEditor(targetObject);
                        if (curEditor == null)
                        {
                            curEditor = TryCreateGenericEditor(targetObject);
                        }
                    }
                    else if (obj is System.Object systemObj)
                    {
                        curEditor = TryCreateGenericNonUnityEditor(systemObj);
                    }
                    _editors[index] = curEditor;
                }
            }

            _currentTargetsImm = new ReadOnlyCollection<object>(_currentPaintedTargets);
        }

        private UnityEditor.Editor TryCreateGenericNonUnityEditor(object systemObj)
        {
            UnityEditor.Editor customEditor = null;
            try
            {
                customEditor = GenericSmartObjectEditor.Create(systemObj);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                customEditor = null;
            }

            return customEditor;
        }

        private static UnityEditor.Editor CreateStandardEditor(UnityEngine.Object targetObject)
        {
            var editor = UnityEditor.Editor.CreateEditor(targetObject);
            if (editor is MaterialEditor matEditor && s_materialForceVisibleProperty != null)
                s_materialForceVisibleProperty.SetValue(matEditor, true, null);
            return editor;
        }

        private UnityEditor.Editor TryCreateGenericEditor(Object targetObject)
        {
            UnityEditor.Editor customEditor = null;
            try
            {
                customEditor = UnityEditor.Editor.CreateEditor(targetObject, typeof(GenericSmartUnityObjectEditor));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                customEditor = null;
            }

            return customEditor;
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
                    if (_editors[i])
                    {
                        DestroyImmediate(_editors[i]);
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

        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            UnityEditor.Editor editor = _editors[index];
            if (editor != null && editor.target != null)
                editor.OnInspectorGUI();

            if (DrawUnityEditorPreview)
                DrawEditorPreview(index, _defaultEditorPreviewHeight);
        }

        /// <summary>
        /// Uses the <see cref="M:UnityEditor.Editor.DrawPreview(UnityEngine.Rect)" /> method to draw a preview for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditorPreview(int index, float height)
        {
            UnityEditor.Editor editor = _editors[index];
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
        
        protected void RequestRepaint()
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