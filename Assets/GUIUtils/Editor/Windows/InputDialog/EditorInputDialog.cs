using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class EditorInputDialog : EditorWindow
    {
        private DialogData _data;

        public float Width = 250;

        // Event.current is not available in a contextmenu function
        // However, it is still available in the code
        // So hack into it and fetch it... why unity (Only way to get mouse position)
        private static FieldInfo _currentEventField =
            typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);


        private void OnGUI()
        {
            if (_data == null)
            {
                Close();
                return;
            }

            EditorGUILayout.BeginVertical(GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(_data.Content);

            foreach (var field in _data.Fields)
                field.Draw(null);

            GUILayout.FlexibleSpace();

            var rect = EditorGUILayout.GetControlRect();
            var confirmRect = AlignLeft(rect, rect.width * 0.48f);
            var cancelRect = AlignRight(rect, rect.width * 0.48f);

            if (GUI.Button(confirmRect, _data.ConfirmButton))
            {
                _data.Resolve(true);
                Close();
            }

            if (!_data.CancelButton.IsNullOrEmpty() && GUI.Button(cancelRect, _data.CancelButton))
            {
                _data.Resolve(false);
                Close();
            }

            EditorGUILayout.EndVertical();
        }

        public static Rect AlignLeft(Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        public static Rect AlignRight(Rect rect, float width)
        {
            rect.x = rect.x + rect.width - width;
            rect.width = width;
            return rect;
        }

        public void SetData(DialogData data)
        {
            titleContent = new GUIContent(data.Title);

            _data = data;
        }

        public void Resize(float width)
        {
            float y = 60 + (_data.Fields.Sum(x => x.Height));
            minSize = new Vector2(width, y);
            maxSize = minSize;
        }

        public void CenterOn(EditorWindow win)
        {
            if (!win) return;

            var pos = position;
            pos.center = win.position.center;
            position = pos;
        }

        /// ================================================================================================================
        /// BUILDER METHODS
        public static DialogBuilder Create(string title, string content, string confirm = "Confirm",
            string cancel = "Cancel")
        {
            return new DialogBuilder(title, content, confirm, cancel);
        }

        public static EditorInputDialog ShowDialog(DialogData data)
        {
            var window = InitWindow(data, out float width);

            window.Resize(width);

            var currentWindow = EditorWindow.focusedWindow;

            window.ShowUtility();

            window.CenterOn(currentWindow);
            return window;
        }

        public static EditorInputDialog ShowOnWindow(DialogData data, Vector2? openPosition = null)
        {
            if (!openPosition.HasValue)
            {
                var currentWindow = EditorWindow.focusedWindow;
                openPosition = currentWindow.position.center;
            }

            return ShowInPopup(data, openPosition);
        }

        public static EditorInputDialog ShowInPopup(DialogData data, Vector2? openPosition = null)
        {
            var window = InitWindow(data, out float x);

            float y = data.GetPreferredHeight();

            if (!openPosition.HasValue)
            {
                Event e = Event.current;
                if (e == null) // See explanation @ _currentEventField
                    e = _currentEventField.GetValue(null) as Event;

                openPosition = e.mousePosition;
                // this is the mousepos relative to the window; offset it
                openPosition += EditorWindow.focusedWindow.position.position;
            }

            // Use the given position or mouse position
            Vector2 position = openPosition.Value;
            Rect btnRect = new Rect(position.x, position.y, 1f, 1f); // Dummy rect
            window.ShowAsDropDown(btnRect, new Vector2(x, y));

            return window;
        }

        public static EditorInputDialog ShowBlocking(DialogData data)
        {
            var window = InitWindow(data, out float width);

            window.Resize(width);

            var currentWindow = EditorWindow.focusedWindow;

            window.ShowModal();

            window.CenterOn(currentWindow);
            return window;
        }

        private static EditorInputDialog InitWindow(DialogData data, out float width)
        {
            var window = CreateInstance<EditorInputDialog>();

            window.SetData(data);

            width = Mathf.Max(window.Width, data.GetPreferredWidth());

            return window;
        }
    }
}