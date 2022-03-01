using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public partial class ToolbarUtility
    {
        // COPY from SirenixEditorGUI.ToolbarButton but with tooltip option
        public static bool ToolbarButton(EditorIcon icon, string tooltip = "", int iconSize = 24)
        {
            Rect rect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayoutOptions.ExpandWidth(false));
            if (GUI.Button(rect, GUIHelper.TempContent(string.Empty, null, tooltip), SirenixGUIStyles.ToolbarButton))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                --rect.y;
                icon.Draw(rect, 16f);
            }

            if (Event.current.button != 0 || Event.current.rawType != EventType.MouseDown ||
                !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                return false;
            GUIHelper.RemoveFocusControl();
            GUIHelper.RequestRepaint();
            GUIHelper.PushGUIEnabled(true);
            Event.current.Use();
            GUIHelper.PopGUIEnabled();
            return true;
        }

        public static bool ToolbarButton(Texture icon, string tooltip = "", int iconSize = 24)
        {
            var content = new GUIContent(icon, tooltip: tooltip);

            return GUILayout.Button(content, SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Height(iconSize).Width(iconSize));
        }
    }
}