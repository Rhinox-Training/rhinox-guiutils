using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public class InlineErrorAttributeDrawer<T> : InlineIconAttributeDrawer<InlineErrorAttribute, T>
    {
        private IfAttributeHelper helper;

        protected override Texture2D Icon => EditorIcons.UnityErrorIcon;
        protected override string Tooltip => Attribute.Text;

        /// <summary>Initializes the drawer.</summary>
        protected override void Initialize()
        {
            if (!this.Attribute.MemberName.IsNullOrWhitespace())
                this.helper = new IfAttributeHelper(this.Property, this.Attribute.MemberName);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!GUI.enabled)
                this.CallNextDrawer(label);

            else if (this.helper == null)
                DrawWithIcon(label);

            else if (this.helper.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(this.helper.ErrorMessage, true);
                this.CallNextDrawer(label);
            }

            else if (this.helper.GetValue(this.Attribute.Value))
            {
                DrawWithIcon(label);
            }
            else
                this.CallNextDrawer(label);
        }
    }
}