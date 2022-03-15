using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class OverrideGroupAttributeDrawer : OdinGroupDrawer<OverrideGroupAttribute>
    {
        private int width = 0;

        protected override void Initialize()
        {
            base.Initialize();

            width = Mathf.RoundToInt(GUI.skin.toggle.lineHeight);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!string.IsNullOrWhiteSpace(Attribute.HideIfMemberName))
            {
                InspectorProperty prop = Property.Parent.FindChild(x => x.Name == Attribute.HideIfMemberName, false);
                if (prop != null && (bool) prop.ValueEntry.WeakSmartValue)
                    return;
            }

            InspectorProperty inspectorProperty = Property.Children.Get(Attribute.ToggleMemberName);

            bool toggle = (bool) inspectorProperty.ValueEntry.WeakSmartValue;

            GUILayout.BeginHorizontal();
            {
                toggle = GUILayout.Toggle(toggle, string.Empty, GUILayoutOptions.Width(width));
                EditorGUI.BeginDisabledGroup(!toggle);
                EditorGUIUtility.labelWidth -= width;


                GUILayout.BeginVertical();
                {
                    for (int index = 0; index < Property.Children.Count; ++index)
                    {
                        InspectorProperty child = Property.Children[index];
                        if (child != inspectorProperty)
                            child.Draw(child.Label);
                    }
                }
                GUILayout.EndVertical();

                EditorGUIUtility.labelWidth += width;
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            inspectorProperty.ValueEntry.WeakSmartValue = toggle;
        }
    }
}