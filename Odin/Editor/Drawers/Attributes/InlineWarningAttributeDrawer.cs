using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class InlineWarningAttributeDrawer<T> : InlineIconAttributeDrawer<InlineWarningAttribute, T>
    {
        private IfAttributeHelper _helper;

        protected override Texture2D Icon => EditorIcons.UnityWarningIcon;
        protected override string Tooltip => Attribute.Text;

        /// <summary>Initializes the drawer.</summary>
        protected override void Initialize()
        {
            if (!this.Attribute.MemberName.IsNullOrWhitespace())
                this._helper = new IfAttributeHelper(this.Property, this.Attribute.MemberName);

            base.Initialize();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!GUI.enabled)
                this.CallNextDrawer(label);

            else if (this._helper == null)
                DrawWithIcon(label);

            else if (this._helper.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this._helper.ErrorMessage, true);
                this.CallNextDrawer(label);
            }

            else if (this._helper.GetValue(this.Attribute.Value))
            {
                DrawWithIcon(label);
            }
            else
                this.CallNextDrawer(label);
        }
    }
}