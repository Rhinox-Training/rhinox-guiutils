using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class ToggleButtonOppositeAttributeDrawer : OdinAttributeDrawer<ToggleButtonOppositeAttribute, bool>
    {
        private bool _doManualColoring = UnityVersion.IsVersionOrGreater(2019, 3);

        private static Color ActiveColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f);
        private static Color InactiveColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;

        private StringMemberHelper OppositeHelper;

        protected override void Initialize()
        {
            base.Initialize();
            OppositeHelper = new StringMemberHelper(Property, Attribute.OppositeName);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (OppositeHelper.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(OppositeHelper.ErrorMessage, true);
                return;
            }

            SirenixEditorGUI.BeginIndentedHorizontal();

            DrawButton(label, true, SirenixGUIStyles.ButtonLeft);
            DrawButton(GUIHelper.TempContent(OppositeHelper.GetString(Property)), false, SirenixGUIStyles.ButtonRight);

            SirenixEditorGUI.EndIndentedHorizontal();
        }

        private void DrawButton(GUIContent label, bool wanted, GUIStyle style)
        {
            if (_doManualColoring)
            {
                var col = ValueEntry.SmartValue == wanted ? ActiveColor : InactiveColor;
                GUIHelper.PushColor(col * GUI.color);
            }

            if (GUILayout.Button(label, style))
            {
                GUIHelper.RemoveFocusControl();
                ValueEntry.SmartValue = wanted;
                GUIHelper.RequestRepaint();
            }

            if (_doManualColoring)
                GUIHelper.PopColor();
        }
    }
}