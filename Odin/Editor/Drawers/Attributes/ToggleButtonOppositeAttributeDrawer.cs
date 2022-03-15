using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class ToggleButtonOppositeAttributeDrawer : OdinAttributeDrawer<ToggleButtonOppositeAttribute, bool>
    {
        private bool _doManualColoring = UnityVersion.IsVersionOrGreater(2019, 3);

        private static Color ActiveColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f);
        private static Color InactiveColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;

        private PropertyMemberHelper<string> _oppositeHelper;

        protected override void Initialize()
        {
            base.Initialize();
            _oppositeHelper = new PropertyMemberHelper<string>(Property, Attribute.OppositeName);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_oppositeHelper.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(_oppositeHelper.ErrorMessage, true);
                return;
            }

            SirenixEditorGUI.BeginIndentedHorizontal();

            DrawButton(label, true, SirenixGUIStyles.ButtonLeft);
            DrawButton(GUIHelper.TempContent(_oppositeHelper.GetValue()), false, SirenixGUIStyles.ButtonRight);

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