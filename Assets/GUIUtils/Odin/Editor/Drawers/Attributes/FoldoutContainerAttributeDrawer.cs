using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class FoldoutContainerAttributeDrawer : OdinGroupDrawer<FoldoutContainerAttribute>
    {
        private LocalPersistentContext<bool> _isVisible;
        private ValueResolver<string> _titleHelper;
        private float _t;

        /// <summary>Initializes this instance.</summary>
        protected override void Initialize()
        {
            _isVisible = this.GetPersistentValue("IsVisible", Attribute.HasDefinedExpanded ? Attribute.Expanded : SirenixEditorGUI.ExpandFoldoutByDefault);
            _titleHelper = ValueResolver.GetForString(Property, Attribute.GroupName);
            _t = _isVisible.Value ? 1f : 0.0f;
        }

        /// <summary>Draws the property.</summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_titleHelper.ErrorMessage != null)
                SirenixEditorGUI.ErrorMessageBox(_titleHelper.ErrorMessage, true);

            var content = GUIHelper.TempContent(_titleHelper.GetValue());

            _isVisible.Value = eUtility.FoldoutHeader(_isVisible.Value, content);

            // update t (for the fadegroup)
            if (Event.current.type == EventType.Layout)
            {
                EditorTimeHelper.Time.Update();
                _t = Mathf.MoveTowards(_t, _isVisible.Value ? 1f : 0.0f, EditorTimeHelper.Time.DeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
            }

            // Draw (BeginFadeGroup handles visibility being 0)
            if (SirenixEditorGUI.BeginFadeGroup(_t))
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