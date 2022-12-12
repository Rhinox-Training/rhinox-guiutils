using System;
using System.Collections.Generic;
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
        private static PropertyInfo materialForceVisibleProperty = typeof(MaterialEditor).GetProperty("forceVisible",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.FlattenHierarchy);

        private static int inspectObjectWindowCount = 3;

        [SerializeField] [HideInInspector] private Object inspectorTargetSerialized;
        [SerializeField] [HideInInspector] private float labelWidth = 0.33f;
        [NonSerialized] private object inspectTargetObject;
        [SerializeField] [HideInInspector] private Vector4 windowPadding = new Vector4(4f, 4f, 4f, 4f);
        [SerializeField] [HideInInspector] private bool useScrollView = true;
        [SerializeField] [HideInInspector] private bool drawUnityEditorPreview;
        [SerializeField] [HideInInspector] private int wrappedAreaMaxHeight = 1000;
        [NonSerialized] private int drawCountWarmup;
        [NonSerialized] private bool isInitialized;
        private GUIStyle marginStyle;
        private object[] currentTargets = new object[0];
        private List<object> currentTargetsImm;
        private UnityEditor.Editor[] editors = new UnityEditor.Editor[0];
        private Vector2 scrollPos;
        private int mouseDownId;
        private EditorWindow mouseDownWindow;
        private int mouseDownKeyboardControl;
        private Vector2 contenSize;
        private float defaultEditorPreviewHeight = 170f;
        private bool preventContentFromExpanding;
        private bool _requestRepaint;

        /// <summary>Occurs when the window is closed.</summary>
        public event Action OnClose;

        /// <summary>Occurs at the beginning the OnGUI method.</summary>
        public event Action OnBeginGUI;

        /// <summary>Occurs at the end the OnGUI method.</summary>
        public event Action OnEndGUI;

        /// <summary>
        /// Gets the label width to be used. Values between 0 and 1 are treated as percentages, and values above as pixels.
        /// </summary>
        public virtual float DefaultLabelWidth
        {
            get => labelWidth;
            set => labelWidth = value;
        }

        /// <summary>
        /// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
        /// </summary>
        public virtual Vector4 WindowPadding
        {
            get => windowPadding;
            set => windowPadding = value;
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a scroll view.
        /// </summary>
        public virtual bool UseScrollView
        {
            get => useScrollView;
            set => useScrollView = true;
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a Unity editor preview, if possible.
        /// </summary>
        public virtual bool DrawUnityEditorPreview
        {
            get => drawUnityEditorPreview;
            set => drawUnityEditorPreview = value;
        }

        /// <summary>Gets the default preview height for Unity editors.</summary>
        public virtual float DefaultEditorPreviewHeight
        {
            get => defaultEditorPreviewHeight;
            set => defaultEditorPreviewHeight = value;
        }

        /// <summary>
        /// Gets the target which which the window is supposed to draw. By default it simply returns the editor window instance itself. By default, this method is called by <see cref="M:Sirenix.OdinInspector.Editor.CustomEditorWindow.GetTargets" />().
        /// </summary>
        protected virtual object GetTarget()
        {
            if (inspectTargetObject != null)
                return inspectTargetObject;
            return inspectorTargetSerialized != null
                ? inspectorTargetSerialized
                : (object)this;
        }

        /// <summary>
        /// Gets the targets to be drawn by the editor window. By default this simply yield returns the <see cref="M:Sirenix.OdinInspector.Editor.CustomEditorWindow.GetTarget" /> method.
        /// </summary>
        protected virtual IEnumerable<object> GetTargets()
        {
            yield return GetTarget();
        }

        /// <summary>
        /// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
        /// </summary>
        protected IReadOnlyList<object> CurrentDrawingTargets => currentTargetsImm;

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(
            object obj,
            Rect btnRect,
            float windowWidth)
        {
            return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0.0f));
        }

        private void SetupAutomaticHeightAdjustment(int maxHeight)
        {
            preventContentFromExpanding = true;
            wrappedAreaMaxHeight = maxHeight;
            int screenHeight = Screen.currentResolution.height - 40;
            Rect originalP = position;
            originalP.x = (int)originalP.x;
            originalP.y = (int)originalP.y;
            originalP.width = (int)originalP.width;
            originalP.height = (int)originalP.height;
            Rect currentP = originalP;
            CustomEditorWindow wnd = this;
            int getGoodOriginalPounter = 0;
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
                if (getGoodOriginalPounter <= 1 && originalP.y < 1.0)
                {
                    ++getGoodOriginalPounter;
                    originalP = position;
                }
                else
                {
                    int y = (int)contenSize.y;
                    if ((double)y != currentP.height)
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
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(
            object obj,
            Rect btnRect,
            Vector2 windowSize)
        {
            CustomEditorWindow window = CreateCustomEditorWindowInstanceForObject(obj);
            if (windowSize.x <= 1.0)
                windowSize.x = btnRect.width;
            if (windowSize.x <= 1.0)
                windowSize.x = 400f;
            btnRect.x = (int)btnRect.x;
            btnRect.width = (int)btnRect.width;
            btnRect.height = (int)btnRect.height;
            btnRect.y = (int)btnRect.y;
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

            if (!EditorGUIUtility.isProSkin)
                window.OnBeginGUI += () =>
                {
                    Rect position = window.position;
                    double width = position.width;
                    position = window.position;
                    double height = position.height;
                    CustomEditorGUI.DrawSolidRect(new Rect(0.0f, 0.0f, (float)width, (float)height),
                        new Color(1f, 1f, 1f, 0.035f));
                };
            window.OnEndGUI += () =>
            {
                Rect position = window.position;
                double width = position.width;
                position = window.position;
                double height = position.height;
                CustomEditorGUI.DrawBorders(new Rect(0.0f, 0.0f, (float)width, (float)height), 1);
            };
            window.labelWidth = 0.33f;
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
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, Vector2 position)
        {
            Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
            return InspectObjectInDropDown(obj, btnRect, 350f);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
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
            Vector2 move = new Vector2(30f, 30f) * (inspectObjectWindowCount++ % 6 - 3);
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
            Object @object = obj as Object;
            if ((bool)@object)
            {
                window.inspectTargetObject = null;
                window.inspectorTargetSerialized = @object;
            }
            else
            {
                window.inspectorTargetSerialized = null;
                window.inspectTargetObject = obj;
            }

            if ((bool)(Object)(@object as Component))
                window.titleContent = new GUIContent((@object as Component).gameObject.name);
            else if ((bool)@object)
                window.titleContent = new GUIContent(@object.name);
            else
                window.titleContent = new GUIContent(obj.ToString());
            EditorUtility.SetDirty(window);
            return window;
        }

        /// <summary>
        /// Creates an editor window instance for the specified object, without opening the window.
        /// </summary>
        public static CustomEditorWindow CreateCustomEditorWindowInstanceForObject(
            object obj)
        {
            CustomEditorWindow instance = CreateInstance<CustomEditorWindow>();
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
            Object @object = obj as Object;
            if ((bool)@object)
                instance.inspectorTargetSerialized = @object;
            else
                instance.inspectTargetObject = obj;
            if ((bool)(Object)(@object as Component))
                instance.titleContent = new GUIContent((@object as Component).gameObject.name);
            else if ((bool)@object)
                instance.titleContent = new GUIContent(@object.name);
            else
                instance.titleContent = new GUIContent(obj.ToString());
            instance.position = RectExtensions.AlignCenter(CustomEditorGUI.GetEditorWindowRect(), 600f, 600f);
            EditorUtility.SetDirty(instance);
            return instance;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            OnBeforeSerialize();
        }

        /// <summary>Draws the Odin Editor Window.</summary>
        protected virtual void OnGUI()
        {
            bool contentFromExpanding = preventContentFromExpanding;
            if (contentFromExpanding)
                GUILayout.BeginArea(new Rect(0.0f, 0.0f, position.width, wrappedAreaMaxHeight));
            if (OnBeginGUI != null)
                OnBeginGUI();

            InitializeIfNeeded();
            GUIStyle guiStyle = marginStyle;
            if (guiStyle == null)
                guiStyle = new GUIStyle
                {
                    padding = new RectOffset()
                };
            marginStyle = guiStyle;
            if (Event.current.type == EventType.Layout)
            {
                marginStyle.padding.left = (int)WindowPadding.x;
                marginStyle.padding.right = (int)WindowPadding.y;
                marginStyle.padding.top = (int)WindowPadding.z;
                marginStyle.padding.bottom = (int)WindowPadding.w;
                UpdateEditors();
            }

            EventType type = Event.current.type;
            if (Event.current.type == EventType.MouseDown)
            {
                mouseDownId = GUIUtility.hotControl;
                mouseDownKeyboardControl = GUIUtility.keyboardControl;
                mouseDownWindow = focusedWindow;
            }

            bool useScrollView = UseScrollView;
            if (useScrollView)
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            Vector2 vector2 = !preventContentFromExpanding
                ? EditorGUILayout.BeginVertical().size
                : EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false)).size;
            if (contenSize == Vector2.zero || Event.current.type == EventType.Repaint)
                contenSize = vector2;
            using (new eUtility.HierarchyMode(false))
            {
                float newLabelWidth = DefaultLabelWidth >= 1.0
                    ? DefaultLabelWidth
                    : contenSize.x * DefaultLabelWidth;
                using (new eUtility.LabelWidth(newLabelWidth))
                {
                    OnBeginDrawEditors();
                    GUILayout.BeginVertical(marginStyle);
                    DrawEditors();
                    GUILayout.EndVertical();
                    OnEndDrawEditors();
                }
            }

            EditorGUILayout.EndVertical();
            if (useScrollView)
                EditorGUILayout.EndScrollView();
            OnEndGUI?.Invoke();
            if (Event.current.type != type)
                mouseDownId = -2;
            if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == mouseDownId &&
                focusedWindow == mouseDownWindow &&
                GUIUtility.keyboardControl == mouseDownKeyboardControl)
            {
                GUIUtility.hotControl = 0;
                DragAndDrop.activeControlID = 0;
                GUIUtility.keyboardControl = 0;
                GUI.FocusControl(null);
            }

            if (drawCountWarmup < 10)
            {
                Repaint();
                if (Event.current.type == EventType.Repaint)
                    ++drawCountWarmup;
            }

            if (Event.current.isMouse || Event.current.type == EventType.Used ||
                currentTargets == null || currentTargets.Length == 0)
                Repaint();
            RepaintIfRequested();
            if (!contentFromExpanding)
                return;
            GUILayout.EndArea();
        }

        protected void RepaintIfRequested()
        {
            if (!_requestRepaint)
                return;
            if (this)
                Repaint();
            _requestRepaint = false;
        }

        /// <summary>
        /// Calls DrawEditor(index) for each of the currently drawing targets.
        /// </summary>
        protected virtual void DrawEditors()
        {
            for (int index = 0; index < currentTargets.Length; ++index)
                DrawEditor(index);
        }

        private void UpdateEditors()
        {
            currentTargets = currentTargets ?? Array.Empty<object>();
            editors = editors ?? Array.Empty<UnityEditor.Editor>();
            IList<object> objectList = GetTargets().ToArray();
            if (currentTargets.Length != objectList.Count)
            {
                if (editors.Length > objectList.Count)
                {
                    int num = editors.Length - objectList.Count;
                    for (int index = 0; index < num; ++index)
                    {
                        UnityEditor.Editor editor = editors[editors.Length - index - 1];
                        if ((bool)(Object)editor)
                            DestroyImmediate(editor);
                    }
                }

                Array.Resize(ref currentTargets, objectList.Count);
                Array.Resize(ref editors, objectList.Count);
                Repaint();
            }

            for (int index = 0; index < objectList.Count; ++index)
            {
                object obj = objectList[index];
                object currentTarget = currentTargets[index];
                if (obj != currentTarget)
                {
                    RequestRepaint();
                    currentTargets[index] = obj;
                    if (obj == null)
                    {
                        if (editors[index])
                            DestroyImmediate(editors[index]);
                        editors[index] = null;
                    }
                    else
                    {
                        if (obj is EditorWindow editorWindow)
                        {
                            var dynamicEntry = TryCreateGenericEditor(editorWindow);

                            if (dynamicEntry == null && editors[index])
                                DestroyImmediate(editors[index]);
                            editors[index] = dynamicEntry;
                        }
                        else
                        {
                            if (TypeExtensions.InheritsFrom<Object>(obj.GetType()))
                            {
                                Object targetObject = obj as Object;
                                if (targetObject)
                                {
                                    if (editors[index])
                                        DestroyImmediate(editors[index]);

                                    var dynamicEntry = UnityEditor.Editor.CreateEditor(targetObject);
                                    if (dynamicEntry == null ||
                                        dynamicEntry.GetType().Name
                                            .Contains("OdinEditor")) // TODO: remove once odin-less testing is done
                                    {
                                        dynamicEntry = TryCreateGenericEditor(targetObject);
                                    }

                                    editors[index] = dynamicEntry;

                                    MaterialEditor editor = editors[index] as MaterialEditor;
                                    if (editor != null &&
                                        materialForceVisibleProperty != null)
                                        materialForceVisibleProperty.SetValue(editor, true, null);
                                }
                                else
                                {
                                    if (editors[index])
                                        DestroyImmediate(editors[index]);
                                    editors[index] = null;
                                }
                            }
                            else
                            {
                                editors[index] = null;
                            }
                        }
                    }
                }
            }

            currentTargetsImm = new List<object>(currentTargets);
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

        protected void RequestRepaint()
        {
            _requestRepaint = true;
        }

        private void InitializeIfNeeded()
        {
            if (isInitialized)
                return;
            isInitialized = true;
            if (titleContent != null && titleContent.text == GetType().FullName)
                titleContent.text = SplitPascalCase(GetNiceName(GetType()));
            wantsMouseMove = true;
            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;
            Initialize();
        }

        // TODO: migrate to lightspeed
        private static string GetNiceName(Type type) => type.IsNested && !type.IsGenericParameter
            ? GetNiceName(type.DeclaringType) + "." + type.Name
            : type.Name;

        public static string SplitPascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            StringBuilder stringBuilder = new StringBuilder(input.Length);
            if (char.IsLetter(input[0]))
                stringBuilder.Append(char.ToUpper(input[0]));
            else
                stringBuilder.Append(input[0]);

            for (int i = 1; i < input.Length; ++i)
            {
                char c = input[i];
                if (char.IsUpper(c) && !char.IsUpper(input[i - 1]))
                    stringBuilder.Append(' ');
                stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }


        /// <summary>
        /// Initialize get called by OnEnable and by OnGUI after assembly reloads
        /// which often happens when you recompile or enter and exit play mode.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        private void SelectionChanged() => Repaint();

        /// <summary>
        /// Called when the window is enabled. Remember to call base.OnEnable();
        /// </summary>
        protected virtual void OnEnable() => InitializeIfNeeded();

        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            UnityEditor.Editor editor = editors[index];
            if (editor != null && editor.target != null)
            {
                editor.OnInspectorGUI();
            }

            if (!DrawUnityEditorPreview)
                return;
            DrawEditorPreview(index, defaultEditorPreviewHeight);
        }

        /// <summary>
        /// Uses the <see cref="M:UnityEditor.Editor.DrawPreview(UnityEngine.Rect)" /> method to draw a preview for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditorPreview(int index, float height)
        {
            UnityEditor.Editor editor = editors[index];
            if (!(editor != null) || !editor.HasPreviewGUI())
                return;
            Rect controlRect = EditorGUILayout.GetControlRect(false, height);
            editor.DrawPreview(controlRect);
        }

        /// <summary>
        /// Called when the window is destroyed. Remember to call base.OnDestroy();
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (editors != null)
            {
                for (int index = 0; index < editors.Length; ++index)
                {
                    if (editors[index])
                    {
                        DestroyImmediate(editors[index]);
                        editors[index] = null;
                    }
                }
            }

            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged -= SelectionChanged;
            if (OnClose == null)
                return;
            OnClose();
        }

        /// <summary>
        /// Called before starting to draw all editors for the <see cref="P:Sirenix.OdinInspector.Editor.CustomEditorWindow.CurrentDrawingTargets" />.
        /// </summary>
        protected virtual void OnEndDrawEditors()
        {
        }

        /// <summary>
        /// Called after all editors for the <see cref="P:Sirenix.OdinInspector.Editor.CustomEditorWindow.CurrentDrawingTargets" /> has been drawn.
        /// </summary>
        protected virtual void OnBeginDrawEditors()
        {
        }

        /// <summary>
        /// See ISerializationCallbackReceiver.OnBeforeSerialize for documentation on how to use this method.
        /// </summary>
        protected virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Implement this method to receive a callback after unity serialized your object.
        /// </summary>
        protected virtual void OnBeforeSerialize()
        {
        }
    }
}