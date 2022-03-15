using System;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class PropertiesGroupDrawer : OdinGroupDrawer<PropertiesGroupAttribute>
    {
        private static GUIStyle _style;

        private object _defaultValue;
        private InspectorProperty _mainProp;

        protected override void Initialize()
        {
            _style = new GUIStyle(SirenixGUIStyles.ToggleGroupTitleBg)
            {
                fixedHeight = 26
            };
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_mainProp == null)
                FetchMainProp();

            if (_mainProp == null)
            {
                SirenixEditorGUI.ErrorMessageBox("PropertiesGroup's RootPropertyName could not be matched to one of its children.");
                return;
            }

            var isExpanded = !Attribute.HideWhenDefault || _mainProp.ValueEntry.WeakSmartValue != _defaultValue;

            SirenixEditorGUI.BeginIndentedHorizontal(_style);
            GUIHelper.PushHierarchyMode(false, true);
            _mainProp.Draw();
            SirenixEditorGUI.EndIndentedHorizontal();
            GUIHelper.PopHierarchyMode();

            GUIHelper.PushHierarchyMode(true, false);

            if (SirenixEditorGUI.BeginFadeGroup(this, isExpanded))
            {
                ++EditorGUI.indentLevel;
                for (int i = 0; i < this.Property.Children.Count; i++)
                {
                    if (_mainProp == Property.Children[i]) continue;

                    this.Property.Children[i].Draw();
                }

                --EditorGUI.indentLevel;
            }

            GUIHelper.PopHierarchyMode();
            SirenixEditorGUI.EndFadeGroup();
        }

        private void FetchMainProp()
        {
            for (var i = 0; i < Property.Children.Count; i++)
            {
                if (Property.Children[i].Name != Attribute.RootPropertyName) continue;
                _mainProp = Property.Children[i];
                break;
            }

            if (_mainProp == null) return;

            var type = _mainProp.ValueEntry.TypeOfValue;
            _defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}