using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0.0, 0.0, 0.11)]
    public class InlinePropertyAltAttributeDrawer : OdinAttributeDrawer<InlinePropertyAltAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            GUILayout.BeginHorizontal();
            if (label == null)
                GUILayout.Space(GUIHelper.CurrentIndentAmount);
            else
            {
                var labelRect = GUILayoutUtility.GetRect(labelWidth - 1, 18f, GUILayoutOptions.ExpandWidth(false));
                float currentIndentAmount = GUIHelper.CurrentIndentAmount;
                labelRect.x += currentIndentAmount;
                labelRect.width -= currentIndentAmount;
                if (labelRect.width < 0.0) labelRect.width = 0.0f;

                GUI.Label(labelRect, label, EditorStyles.label);
            }

            EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandWidth(true));

            GUIHelper.PushLabelWidth(labelWidth - GUIHelper.CurrentIndentAmount);
            GUIHelper.PushIndentLevel(0);

            // EditorGUI.DrawRect(controlRect, Color.red);

            foreach (var p in Property.Children)
                p.Draw();

            // this.CallNextDrawer(null);

            GUIHelper.PopLabelWidth();
            GUIHelper.PopIndentLevel();
            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}