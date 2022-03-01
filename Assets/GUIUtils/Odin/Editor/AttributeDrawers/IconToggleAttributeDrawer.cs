using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class IconToggleAttributeDrawer : OdinAttributeDrawer<IconToggleAttribute, bool>
    {
        private static Color ActiveColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f);
        private static Color InactiveColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;

        private EditorIcon _trueIcon;
        private EditorIcon _falseIcon;

        protected override void Initialize()
        {
            const BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public;
            var t = typeof(EditorIcons);

            _trueIcon = t.GetProperty(Attribute.TrueIcon, bindingAttr)?.GetValue(null) as EditorIcon;
            _falseIcon = t.GetProperty(Attribute.FalseIcon, bindingAttr)?.GetValue(null) as EditorIcon;

            if (_trueIcon == null) _trueIcon = EditorIcons.Checkmark;
            if (_falseIcon == null) _falseIcon = EditorIcons.X;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var col = ValueEntry.SmartValue ? ActiveColor : InactiveColor;
            GUIHelper.PushColor(col * GUI.color);

            if (SirenixEditorGUI.IconButton(ValueEntry.SmartValue ? _trueIcon.Active : _falseIcon.Inactive, tooltip: label?.tooltip))
                ValueEntry.SmartValue = !ValueEntry.SmartValue;

            GUIHelper.PopColor();
        }
    }
}