using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class BoldLabelAttributeDrawer : OdinAttributeDrawer<BoldLabelAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUIHelper.PushIsBoldLabel(true);
            this.CallNextDrawer(label);
            GUIHelper.PopIsBoldLabel();
        }
    }
}