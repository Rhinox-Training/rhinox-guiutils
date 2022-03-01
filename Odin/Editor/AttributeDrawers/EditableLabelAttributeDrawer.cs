using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class EditableLabelAttributeDrawer : OdinAttributeDrawer<EditableLabelAttribute>
    {
        private GUIContent overrideLabel;
        private InspectorProperty labelProperty;

        private Func<object, object> valueGetter;
        private Action<object, object> valueSetter;

        private LocalPersistentContext<bool> isEditing;
        private LocalPersistentContext<string> editValue;

        protected override void Initialize()
        {
            var path = this.Attribute.LabelProperty;
            labelProperty = Property.Parent.FindChild(x => x.Path == this.Attribute.LabelProperty, false);

            var t = Property.Parent.Info.TypeOfValue;
            valueGetter = DeepReflection.CreateWeakInstanceValueGetter(t, typeof(string), path, true);
            valueSetter = DeepReflection.CreateWeakInstanceValueSetter(t, typeof(string), path, true);

            isEditing = this.GetPersistentValue(nameof(isEditing), false);
            isEditing.Value = false;
            editValue = this.GetPersistentValue(nameof(editValue), "");
        }

        /// <summary>Draws the attribute.</summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (labelProperty != null)
                DrawLabelProperty();
            else if (valueGetter != null && valueSetter != null)
                DrawLabelField();
            else
            {
                SirenixEditorGUI.ErrorMessageBox($"Could not find property '{Attribute.LabelProperty}'.", true);
                this.CallNextDrawer(null);
            }
        }

        private void DrawLabelField()
        {
            var inst = Property.Parent.ValueEntry.WeakSmartValue;
            var str = (string) valueGetter.Invoke(inst);

            ContinueDraw(str);
            var applyEdit = HandleIsEditing(str);

            if (applyEdit)
                valueSetter.Invoke(inst, editValue.Value);
        }

        private void DrawLabelProperty()
        {
            if (labelProperty.Info.TypeOfValue != typeof(string))
                SirenixEditorGUI.ErrorMessageBox($"Property '{Attribute.LabelProperty}' needs to be a string.", true);

            var str = (string) labelProperty.ValueEntry.WeakSmartValue;

            ContinueDraw(str);
            var applyEdit = HandleIsEditing(str);

            if (applyEdit)
                labelProperty.ValueEntry.WeakSmartValue = editValue.Value;
        }

        private bool HandleIsEditing(string currentValue)
        {
            var rect = GUILayoutUtility.GetLastRect();
            rect.width = EditorGUIUtility.labelWidth - 2;

            var e = Event.current;

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition) && e.clickCount >= 2)
            {
                isEditing.Value = true;
                editValue.Value = currentValue;
            }

            if (!isEditing.Value) return false;

            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    // if enter, return that the value must be applied
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        isEditing.Value = false;
                        return true;
                    // if escape stop editing and revert value
                    case KeyCode.Escape:
                        isEditing.Value = false;
                        GUIHelper.CurrentWindow?.Repaint();
                        return false;
                }
            }

            editValue.Value = EditorGUI.TextField(rect, GUIContent.none, editValue.Value);

            return false;
        }

        private void ContinueDraw(string str)
        {
            EditorGUI.BeginDisabledGroup(isEditing.Value);
            {
                GUIContent newLabel;
                if (str == null)
                {
                    newLabel = (GUIContent) null;
                }
                else
                {
                    if (this.overrideLabel == null)
                        this.overrideLabel = new GUIContent();
                    this.overrideLabel.text = str;
                    newLabel = this.overrideLabel;
                }

                this.CallNextDrawer(newLabel);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}