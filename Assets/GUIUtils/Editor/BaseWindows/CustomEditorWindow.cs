using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using RectExtensions = Rhinox.Lightspeed.RectExtensions;
using TypeExtensions = Rhinox.Lightspeed.Reflection.TypeExtensions;

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
            get => this.labelWidth;
            set => this.labelWidth = value;
        }

        /// <summary>
        /// Gets or sets the window padding. x = left, y = right, z = top, w = bottom.
        /// </summary>
        public virtual Vector4 WindowPadding
        {
            get => this.windowPadding;
            set => this.windowPadding = value;
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a scroll view.
        /// </summary>
        public virtual bool UseScrollView
        {
            get => this.useScrollView;
            set => this.useScrollView = true;
        }

        /// <summary>
        /// Gets a value indicating whether the window should draw a Unity editor preview, if possible.
        /// </summary>
        public virtual bool DrawUnityEditorPreview
        {
            get => this.drawUnityEditorPreview;
            set => this.drawUnityEditorPreview = value;
        }

        /// <summary>Gets the default preview height for Unity editors.</summary>
        public virtual float DefaultEditorPreviewHeight
        {
            get => this.defaultEditorPreviewHeight;
            set => this.defaultEditorPreviewHeight = value;
        }

        /// <summary>
        /// Gets the target which which the window is supposed to draw. By default it simply returns the editor window instance itself. By default, this method is called by <see cref="M:Sirenix.OdinInspector.Editor.CustomEditorWindow.GetTargets" />().
        /// </summary>
        protected virtual object GetTarget()
        {
            if (this.inspectTargetObject != null)
                return this.inspectTargetObject;
            return this.inspectorTargetSerialized != (Object) null
                ? (object) this.inspectorTargetSerialized
                : (object) this;
        }

        /// <summary>
        /// Gets the targets to be drawn by the editor window. By default this simply yield returns the <see cref="M:Sirenix.OdinInspector.Editor.CustomEditorWindow.GetTarget" /> method.
        /// </summary>
        protected virtual IEnumerable<object> GetTargets()
        {
            yield return this.GetTarget();
        }

        /// <summary>
        /// At the start of each OnGUI event when in the Layout event, the GetTargets() method is called and cached into a list which you can access from here.
        /// </summary>
        protected IReadOnlyList<object> CurrentDrawingTargets => this.currentTargetsImm;

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
            return CustomEditorWindow.InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0.0f));
        }

        private void SetupAutomaticHeightAdjustment(int maxHeight)
        {
            this.preventContentFromExpanding = true;
            this.wrappedAreaMaxHeight = maxHeight;
            int screenHeight = Screen.currentResolution.height - 40;
            Rect originalP = this.position;
            originalP.x = (float) (int) originalP.x;
            originalP.y = (float) (int) originalP.y;
            originalP.width = (float) (int) originalP.width;
            originalP.height = (float) (int) originalP.height;
            Rect currentP = originalP;
            CustomEditorWindow wnd = this;
            int getGoodOriginalPounter = 0;
            int tmpFrameCount = 0;
            EditorApplication.CallbackFunction callback = (EditorApplication.CallbackFunction) null;
            callback = (EditorApplication.CallbackFunction) (() =>
            {
                EditorApplication.update -= callback;
                EditorApplication.update -= callback;
                if ((Object) wnd == (Object) null)
                    return;
                if (tmpFrameCount++ < 10)
                    wnd.Repaint();
                if (getGoodOriginalPounter <= 1 && (double) originalP.y < 1.0)
                {
                    ++getGoodOriginalPounter;
                    originalP = this.position;
                }
                else
                {
                    int y = (int) this.contenSize.y;
                    if ((double) y != (double) currentP.height)
                    {
                        tmpFrameCount = 0;
                        currentP = originalP; // Copy with changed height
                        currentP.height = (float) Math.Min(y, maxHeight);

                        wnd.minSize = new Vector2(wnd.minSize.x, currentP.height);
                        wnd.maxSize = new Vector2(wnd.maxSize.x, currentP.height);
                        if ((double) currentP.yMax >= (double) screenHeight)
                            currentP.y -= currentP.yMax - (float) screenHeight;
                        wnd.position = currentP;
                    }
                }

                EditorApplication.update += callback;
            });
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
            CustomEditorWindow window = CustomEditorWindow.CreateCustomEditorWindowInstanceForObject(obj);
            if ((double) windowSize.x <= 1.0)
                windowSize.x = btnRect.width;
            if ((double) windowSize.x <= 1.0)
                windowSize.x = 400f;
            btnRect.x = (float) (int) btnRect.x;
            btnRect.width = (float) (int) btnRect.width;
            btnRect.height = (float) (int) btnRect.height;
            btnRect.y = (float) (int) btnRect.y;
            windowSize.x = (float) (int) windowSize.x;
            windowSize.y = (float) (int) windowSize.y;
            try
            {
                EditorWindow curr = CustomEditorGUI.CurrentWindow();
                if ((Object) curr != (Object) null)
                    window.OnBeginGUI += (Action) (() => curr.Repaint());
            }
            catch
            {
            }

            if (!EditorGUIUtility.isProSkin)
                window.OnBeginGUI += (Action) (() =>
                {
                    Rect position = window.position;
                    double width = (double) position.width;
                    position = window.position;
                    double height = (double) position.height;
                    CustomEditorGUI.DrawSolidRect(new Rect(0.0f, 0.0f, (float) width, (float) height),
                        new Color(1f, 1f, 1f, 0.035f));
                });
            window.OnEndGUI += (Action) (() =>
            {
                Rect position = window.position;
                double width = (double) position.width;
                position = window.position;
                double height = (double) position.height;
                CustomEditorGUI.DrawBorders(new Rect(0.0f, 0.0f, (float) width, (float) height), 1);
            });
            window.labelWidth = 0.33f;
            window.DrawUnityEditorPreview = true;
            btnRect.position = GUIUtility.GUIToScreenPoint(btnRect.position);
            if ((int) windowSize.y == 0)
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
        public static CustomEditorWindow InspectObjectInDropDown(
            object obj,
            Vector2 position)
        {
            Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
            return CustomEditorWindow.InspectObjectInDropDown(obj, btnRect, 350f);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(
            object obj,
            float windowWidth)
        {
            Vector2 mousePosition = Event.current.mousePosition;
            Rect btnRect = new Rect(mousePosition.x, mousePosition.y, 1f, 1f);
            return CustomEditorWindow.InspectObjectInDropDown(obj, btnRect, windowWidth);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(
            object obj,
            Vector2 position,
            float windowWidth)
        {
            Rect btnRect = new Rect(position.x, position.y, 1f, 1f);
            return CustomEditorWindow.InspectObjectInDropDown(obj, btnRect, windowWidth);
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(
            object obj,
            float width,
            float height)
        {
            Rect btnRect = new Rect(Event.current.mousePosition, Vector2.one);
            return CustomEditorWindow.InspectObjectInDropDown(obj, btnRect, new Vector2(width, height));
        }

        /// <summary>
        /// <para>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// </para>
        /// <para>Protip: You can subscribe to OnClose if you want to know when that occurs.</para>
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj) =>
            CustomEditorWindow.InspectObjectInDropDown(obj, Event.current.mousePosition);

        /// <summary>Pops up an editor window for the given object.</summary>
        public static CustomEditorWindow InspectObject(object obj)
        {
            CustomEditorWindow instanceForObject = CustomEditorWindow.CreateCustomEditorWindowInstanceForObject(obj);
            instanceForObject.Show();
            Vector2 move = new Vector2(30f, 30f) * (float) (CustomEditorWindow.inspectObjectWindowCount++ % 6 - 3);
            var baseRect = RectExtensions.AlignCenter(CustomEditorGUI.GetEditorWindowRect(), 400f, 300f);
            baseRect.position += move;
            instanceForObject.position = baseRect;
            return instanceForObject;
        }

        /// <summary>
        /// Inspects the object using an existing CustomEditorWindow.
        /// </summary>
        public static CustomEditorWindow InspectObject(
            CustomEditorWindow window,
            object obj)
        {
            Object @object = obj as Object;
            if ((bool) @object)
            {
                window.inspectTargetObject = (object) null;
                window.inspectorTargetSerialized = @object;
            }
            else
            {
                window.inspectorTargetSerialized = (Object) null;
                window.inspectTargetObject = obj;
            }

            if ((bool) (Object) (@object as Component))
                window.titleContent = new GUIContent((@object as Component).gameObject.name);
            else if ((bool) @object)
                window.titleContent = new GUIContent(@object.name);
            else
                window.titleContent = new GUIContent(obj.ToString());
            EditorUtility.SetDirty((Object) window);
            return window;
        }

        /// <summary>
        /// Creates an editor window instance for the specified object, without opening the window.
        /// </summary>
        public static CustomEditorWindow CreateCustomEditorWindowInstanceForObject(
            object obj)
        {
            CustomEditorWindow instance = ScriptableObject.CreateInstance<CustomEditorWindow>();
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
            Object @object = obj as Object;
            if ((bool) @object)
                instance.inspectorTargetSerialized = @object;
            else
                instance.inspectTargetObject = obj;
            if ((bool) (Object) (@object as Component))
                instance.titleContent = new GUIContent((@object as Component).gameObject.name);
            else if ((bool) @object)
                instance.titleContent = new GUIContent(@object.name);
            else
                instance.titleContent = new GUIContent(obj.ToString());
            instance.position = RectExtensions.AlignCenter(CustomEditorGUI.GetEditorWindowRect(), 600f, 600f);
            EditorUtility.SetDirty((Object) instance);
            return instance;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.OnBeforeSerialize();
        }

        /// <summary>Draws the Odin Editor Window.</summary>
        protected virtual void OnGUI()
        {
            bool contentFromExpanding = this.preventContentFromExpanding;
            if (contentFromExpanding)
                GUILayout.BeginArea(new Rect(0.0f, 0.0f, this.position.width, (float) this.wrappedAreaMaxHeight));
            if (this.OnBeginGUI != null)
                this.OnBeginGUI();
            
            this.InitializeIfNeeded();
            GUIStyle guiStyle = this.marginStyle;
            if (guiStyle == null)
                guiStyle = new GUIStyle()
                {
                    padding = new RectOffset()
                };
            this.marginStyle = guiStyle;
            if (Event.current.type == UnityEngine.EventType.Layout)
            {
                this.marginStyle.padding.left = (int) this.WindowPadding.x;
                this.marginStyle.padding.right = (int) this.WindowPadding.y;
                this.marginStyle.padding.top = (int) this.WindowPadding.z;
                this.marginStyle.padding.bottom = (int) this.WindowPadding.w;
                this.UpdateEditors();
            }

            UnityEngine.EventType type = Event.current.type;
            if (Event.current.type == UnityEngine.EventType.MouseDown)
            {
                this.mouseDownId = GUIUtility.hotControl;
                this.mouseDownKeyboardControl = GUIUtility.keyboardControl;
                this.mouseDownWindow = EditorWindow.focusedWindow;
            }

            bool useScrollView = this.UseScrollView;
            if (useScrollView)
                this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);
            Vector2 vector2 = !this.preventContentFromExpanding
                ? EditorGUILayout.BeginVertical().size
                : EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false)).size;
            if (this.contenSize == Vector2.zero || Event.current.type == UnityEngine.EventType.Repaint)
                this.contenSize = vector2;
            using (new eUtility.HierarchyMode(false))
            {
                float newLabelWidth = (double) this.DefaultLabelWidth >= 1.0
                    ? this.DefaultLabelWidth
                    : this.contenSize.x * this.DefaultLabelWidth;
                using (new eUtility.LabelWidth(newLabelWidth))
                {
                    this.OnBeginDrawEditors();
                    GUILayout.BeginVertical(this.marginStyle);
                    this.DrawEditors();
                    GUILayout.EndVertical();
                    this.OnEndDrawEditors();
                }
            }

            EditorGUILayout.EndVertical();
            if (useScrollView)
                EditorGUILayout.EndScrollView();
            if (this.OnEndGUI != null)
                this.OnEndGUI();
            if (Event.current.type != type)
                this.mouseDownId = -2;
            if (Event.current.type == UnityEngine.EventType.MouseUp && GUIUtility.hotControl == this.mouseDownId &&
                (Object) EditorWindow.focusedWindow == (Object) this.mouseDownWindow &&
                GUIUtility.keyboardControl == this.mouseDownKeyboardControl)
            {
                GUIUtility.hotControl = 0;
                DragAndDrop.activeControlID = 0;
                GUIUtility.keyboardControl = 0;
                GUI.FocusControl((string) null);
            }

            if (this.drawCountWarmup < 10)
            {
                this.Repaint();
                if (Event.current.type == UnityEngine.EventType.Repaint)
                    ++this.drawCountWarmup;
            }

            if (Event.current.isMouse || Event.current.type == UnityEngine.EventType.Used ||
                this.currentTargets == null || this.currentTargets.Length == 0)
                this.Repaint();
            RepaintIfRequested();
            if (!contentFromExpanding)
                return;
            GUILayout.EndArea();
        }

        protected void RepaintIfRequested()
        {
            if (!_requestRepaint)
                return;
            if ((bool) (UnityEngine.Object) this)
                Repaint();
            _requestRepaint = false;
        }

        /// <summary>
        /// Calls DrawEditor(index) for each of the currently drawing targets.
        /// </summary>
        protected virtual void DrawEditors()
        {
            for (int index = 0; index < this.currentTargets.Length; ++index)
                this.DrawEditor(index);
        }

        private void UpdateEditors()
        {
            this.currentTargets = this.currentTargets ?? new object[0];
            this.editors = this.editors ?? new UnityEditor.Editor[0];
            IList<object> objectList = (IList<object>) (this.GetTargets().ToArray<object>() ?? new object[0]);
            if (this.currentTargets.Length != objectList.Count)
            {
                if (this.editors.Length > objectList.Count)
                {
                    int num = this.editors.Length - objectList.Count;
                    for (int index = 0; index < num; ++index)
                    {
                        UnityEditor.Editor editor = this.editors[this.editors.Length - index - 1];
                        if ((bool) (Object) editor)
                            Object.DestroyImmediate((Object) editor);
                    }
                }

                Array.Resize<object>(ref this.currentTargets, objectList.Count);
                Array.Resize<UnityEditor.Editor>(ref this.editors, objectList.Count);
                this.Repaint();
            }

            for (int index = 0; index < objectList.Count; ++index)
            {
                object obj = objectList[index];
                object currentTarget = this.currentTargets[index];
                if (obj != currentTarget)
                {
                    RequestRepaint();
                    this.currentTargets[index] = obj;
                    if (obj == null)
                    {
                        if ((bool) (Object) this.editors[index])
                            Object.DestroyImmediate((Object) this.editors[index]);
                        this.editors[index] = (UnityEditor.Editor) null;
                    }
                    else
                    {
                        EditorWindow editorWindow = obj as EditorWindow;
                        if (TypeExtensions.InheritsFrom<Object>(obj.GetType()) && !(bool) (Object) editorWindow)
                        {
                            Object targetObject = obj as Object;
                            if ((bool) targetObject)
                            {
                                if ((bool) (Object) this.editors[index])
                                    Object.DestroyImmediate((Object) this.editors[index]);
                                this.editors[index] = UnityEditor.Editor.CreateEditor(targetObject);
                                MaterialEditor editor = this.editors[index] as MaterialEditor;
                                if ((Object) editor != (Object) null &&
                                    CustomEditorWindow.materialForceVisibleProperty != null)
                                    CustomEditorWindow.materialForceVisibleProperty.SetValue((object) editor,
                                        (object) true, (object[]) null);
                            }
                            else
                            {
                                if ((bool) (Object) this.editors[index])
                                    Object.DestroyImmediate((Object) this.editors[index]);
                                this.editors[index] = (UnityEditor.Editor) null;
                            }
                        }
                        else
                        {
                            if ((bool) (Object) this.editors[index])
                                Object.DestroyImmediate((Object) this.editors[index]);
                            this.editors[index] = (UnityEditor.Editor) null;
                        }
                    }
                }
            }

            this.currentTargetsImm = new List<object>((IList<object>) this.currentTargets);
        }

        protected void RequestRepaint()
        {
            _requestRepaint = true;
        }

        private void InitializeIfNeeded()
        {
            if (this.isInitialized)
                return;
            this.isInitialized = true;
            if (this.titleContent != null && this.titleContent.text == this.GetType().FullName)
                this.titleContent.text = SplitPascalCase(GetNiceName(this.GetType()));
            this.wantsMouseMove = true;
            Selection.selectionChanged -= new Action(this.SelectionChanged);
            Selection.selectionChanged += new Action(this.SelectionChanged);
            this.Initialize();
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

        private void SelectionChanged() => this.Repaint();

        /// <summary>
        /// Called when the window is enabled. Remember to call base.OnEnable();
        /// </summary>
        protected virtual void OnEnable() => this.InitializeIfNeeded();

        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            UnityEditor.Editor editor = this.editors[index];
            if ((Object) editor != (Object) null && editor.target != (Object) null)
            {
                editor.OnInspectorGUI();
            }

            if (!this.DrawUnityEditorPreview)
                return;
            this.DrawEditorPreview(index, this.defaultEditorPreviewHeight);
        }

        /// <summary>
        /// Uses the <see cref="M:UnityEditor.Editor.DrawPreview(UnityEngine.Rect)" /> method to draw a preview for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditorPreview(int index, float height)
        {
            UnityEditor.Editor editor = this.editors[index];
            if (!((Object) editor != (Object) null) || !editor.HasPreviewGUI())
                return;
            Rect controlRect = EditorGUILayout.GetControlRect(false, height);
            editor.DrawPreview(controlRect);
        }

        /// <summary>
        /// Called when the window is destroyed. Remember to call base.OnDestroy();
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (this.editors != null)
            {
                for (int index = 0; index < this.editors.Length; ++index)
                {
                    if ((bool) (Object) this.editors[index])
                    {
                        Object.DestroyImmediate((Object) this.editors[index]);
                        this.editors[index] = (UnityEditor.Editor) null;
                    }
                }
            }

            Selection.selectionChanged -= new Action(this.SelectionChanged);
            Selection.selectionChanged -= new Action(this.SelectionChanged);
            if (this.OnClose == null)
                return;
            this.OnClose();
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