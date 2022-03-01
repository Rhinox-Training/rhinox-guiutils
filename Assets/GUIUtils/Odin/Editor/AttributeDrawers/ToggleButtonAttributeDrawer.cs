using System;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class ToggleButtonAttributeDrawer : OdinAttributeDrawer<ToggleButtonAttribute, bool>
    {
        private bool _doManualColoring = UnityVersion.IsVersionOrGreater(2019, 3);

        private static Color ActiveColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f);
        private static Color InactiveColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_doManualColoring)
            {
                var col = ValueEntry.SmartValue ? ActiveColor : InactiveColor;
                GUIHelper.PushColor(col * GUI.color);
            }

            var style = Property.Context.GetGlobal("ButtonStyle", SirenixGUIStyles.Button).Value;
            if (GUILayout.Button(label, style))
            {
                GUIHelper.RemoveFocusControl();
                ValueEntry.SmartValue = !ValueEntry.SmartValue;
                GUIHelper.RequestRepaint();
            }

            if (_doManualColoring)
                GUIHelper.PopColor();
        }
    }
}