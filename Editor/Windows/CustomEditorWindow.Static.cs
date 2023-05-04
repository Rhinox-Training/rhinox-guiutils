using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public partial class CustomEditorWindow
    {
        /// <summary>
        /// Pops up an editor window for the given object in a drop-down window which closes when it loses its focus.
        /// This particular overload uses a few frames to calculate the height of the content before showing the window with a height that matches its content.
        /// </summary>
        public static CustomEditorWindow InspectObjectInDropDown(object obj, Rect btnRect, float windowWidth)
        {
            return InspectObjectInDropDown(obj, btnRect, new Vector2(windowWidth, 0.0f));
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

            window.titleContent = CustomGUIUtility.CreateGUIContentForObject(obj);
            EditorUtility.SetDirty(window);
            return window;
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
            
            instance.titleContent = CustomGUIUtility.CreateGUIContentForObject(obj);
            instance.position = RectExtensions.AlignCenter(CustomEditorGUI.GetEditorWindowRect(), 600f, 600f);
            EditorUtility.SetDirty(instance);
            return instance;
        }
    }
}