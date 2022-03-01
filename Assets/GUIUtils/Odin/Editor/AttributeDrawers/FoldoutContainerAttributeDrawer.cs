using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class FoldoutContainerAttributeDrawer : OdinGroupDrawer<FoldoutContainerAttribute>
    {
        private LocalPersistentContext<bool> IsVisible;
        private StringMemberHelper TitleHelper;
        private float t;

        /// <summary>Initializes this instance.</summary>
        protected override void Initialize()
        {
            IsVisible = this.GetPersistentValue<bool>("IsVisible",
                this.Attribute.HasDefinedExpanded ? this.Attribute.Expanded : SirenixEditorGUI.ExpandFoldoutByDefault);
            TitleHelper = new StringMemberHelper(this.Property, this.Attribute.GroupName);
            t = IsVisible.Value ? 1f : 0.0f;
        }

        /// <summary>Draws the property.</summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.TitleHelper.ErrorMessage != null)
                SirenixEditorGUI.ErrorMessageBox(this.TitleHelper.ErrorMessage, true);

            var content = GUIHelper.TempContent(this.TitleHelper.GetString(Property));

            IsVisible.Value = eUtility.FoldoutHeader(IsVisible.Value, content);

            // update t (for the fadegroup)
            if (Event.current.type == EventType.Layout)
            {
                EditorTimeHelper.Time.Update();
                t = Mathf.MoveTowards(this.t, this.IsVisible.Value ? 1f : 0.0f,
                    EditorTimeHelper.Time.DeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
            }

            // Draw (BeginFadeGroup handles visibility being 0)
            if (SirenixEditorGUI.BeginFadeGroup(this.t))
            {
                for (int index = 0; index < Property.Children.Count; ++index)
                {
                    InspectorProperty child = Property.Children[index];
                    child.Draw(child.Label);
                }
            }

            SirenixEditorGUI.EndFadeGroup();

            GUILayout.Space(1);
        }
    }
}