using System.Collections;
using Sirenix.OdinInspector.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public class OptionalValueAttributeDrawer : OdinAttributeDrawer<OptionalValueAttribute>
    {
        private GUIContent _iconContent;

        private object _defaultValue;
        private Object _defaultUnityValue; // unity fake null object proofing
        private Color _inactiveColor;

        private bool _isUnityObject;

        protected override void Initialize()
        {
            var tooltip = Attribute.Tooltip.IsNullOrEmpty() ? "Optional" : Attribute.Tooltip;
            _iconContent = new GUIContent(EditorIcons.SpeechBubbleRound.Active, tooltip);

            _isUnityObject = Property.ValueEntry.TypeOfValue.InheritsFrom(typeof(UnityEngine.Object));
            _defaultUnityValue = null;
            _defaultValue = Property.ValueEntry.TypeOfValue.GetDefault();

            const float greyScale = .77f;
            _inactiveColor = new Color(greyScale, greyScale + .05f, greyScale, 1f);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            bool hasValue;
            if (_isUnityObject) // Unity object has some fake null shenanigans; this goes around it
                hasValue = (Object) Property.ValueEntry.WeakSmartValue != _defaultUnityValue;
            else hasValue = !Equals(_defaultValue, Property.ValueEntry.WeakSmartValue);

            if (!hasValue)
                GUIHelper.PushColor(_inactiveColor * GUI.color);

            var rect = EditorGUILayout.BeginHorizontal();
            CallNextDrawer(label);

            const float iconSize = 18;
            var iconRect = rect.AlignLeft(EditorGUIUtility.labelWidth).AlignRight(iconSize);

            //  EditorIcons.SpeechBubbleRound.Draw(iconRect, iconSize);

            GUI.Label(iconRect, _iconContent);

            EditorGUILayout.EndHorizontal();

            if (!hasValue)
                GUIHelper.PopColor();
        }
    }
}