using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class ToggledByAttributeDrawer : OdinAttributeDrawer<ToggledByAttribute>
    {
        private InspectorProperty _boolMember;

        private string _errorMessage;

        protected override void Initialize()
        {
            _boolMember = Property.FindSibling(Attribute.ToggleMember);

            if (_boolMember == null)
                _errorMessage = $"Member '{Attribute.ToggleMember}' not found...";
            else if (_boolMember.ValueEntry.TypeOfValue != typeof(bool))
                _errorMessage = $"ToggleMember must be of type 'bool'.";

            base.Initialize();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                SirenixEditorGUI.ErrorMessageBox(_errorMessage);
                CallNextDrawer(label);
                return;
            }

            if (_boolMember == null)
            {
                CallNextDrawer(label);
                return;
            }

            var rect = EditorGUILayout.BeginHorizontal();

            var indent = GUIHelper.CurrentIndentAmount;
            float labelSize = EditorGUIUtility.labelWidth;
            float size = EditorGUIUtility.singleLineHeight;

            // Mark the space used up by calls outside of the layout system that we will use after
            GUILayout.Space(labelSize - indent);

            // Calc rects for the parts
            Rect labelRect = new Rect(rect.x + indent, rect.y, labelSize - indent, size);
            Rect boolRect = labelRect.AlignLeft(size);
            labelRect = labelRect.HorizontalPadding(size, 0f);

            bool state = (bool) _boolMember.ValueEntry.WeakSmartValue;
            _boolMember.ValueEntry.WeakSmartValue = EditorGUI.Toggle(boolRect, GUIContent.none, state);

            if (Attribute.MakeReadOnly)
                GUIHelper.PushGUIEnabled(state && GUI.enabled);

            // Draw the label
            GUI.Label(labelRect, label);

            // Draw the main field
            GUILayout.BeginVertical();
            CallNextDrawer(null);
            GUILayout.EndVertical();

            if (Attribute.MakeReadOnly)
                GUIHelper.PopGUIEnabled();

            EditorGUILayout.EndHorizontal();
        }
    }
}