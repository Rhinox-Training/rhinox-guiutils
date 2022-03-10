using System;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class ToggleButtonGroupDrawer : OdinGroupDrawer<ToggleButtonAttribute>
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return type == typeof(bool);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.BeginIndentedHorizontal();

            for (int i = 0; i < Property.Children.Count; ++i)
            {
                var val = (bool) Property.Children[i].ValueEntry.WeakSmartValue;
                GUIStyle style = eUtility.GetButtonGroupStyle(i, Property.Children.Count, val);
                InspectorProperty child = Property.Children[i];
                child.Context.GetGlobal("ButtonStyle", style).Value = style;
                child.Draw();
            }

            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}