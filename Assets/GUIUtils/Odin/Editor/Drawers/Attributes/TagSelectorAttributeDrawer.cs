using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class TagSelectorAttributeDrawer : OdinAttributeDrawer<TagSelectorAttribute, string>
    {
        private bool currentValueMissing;
        private string[] tags = null;

        private void RefreshTagList()
        {
            this.tags = UnityEditorInternal.InternalEditorUtility.tags;
        }

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        protected override void Initialize()
        {
            RefreshTagList();
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Event.current.type == EventType.Layout)
            {
                this.currentValueMissing = string.IsNullOrEmpty(this.ValueEntry.SmartValue) == false &&
                                           this.tags.Contains(this.ValueEntry.SmartValue) == false;
            }

            if (this.currentValueMissing)
            {
                SirenixEditorGUI.ErrorMessageBox($"The tag '{ValueEntry.SmartValue}' does not exist.");
            }

            var result = GenericSelector<string>.DrawSelectorDropdown(
                label,
                ValueEntry.SmartValue.DefaultTo("<No tag>"),
                this.CreateSelector,
                SirenixGUIStyles.Popup);

            if (result != null)
            {
                this.ValueEntry.SmartValue = result.FirstOrDefault();
            }
        }

        private OdinSelector<string> CreateSelector(Rect position)
        {
            this.RefreshTagList();
            var selector = new GenericSelector<string>(this.tags);

            if (this.tags.Contains(this.ValueEntry.SmartValue))
            {
                selector.SetSelection(this.ValueEntry.SmartValue);
            }

            selector.ShowInPopup(position);

            return selector;
        }
    }
}