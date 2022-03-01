using Rhinox.Utilities;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class FittedLabelAttributeDrawer : OdinAttributeDrawer<FittedLabelAttribute>
    {
        private StringMemberHelper stringHelper;
    
        protected override void Initialize()
        {
            this.stringHelper = new StringMemberHelper(this.Property, this.Attribute.Text);
        }
    
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (stringHelper.ErrorMessage != null)
                SirenixEditorGUI.ErrorMessageBox(stringHelper.ErrorMessage);
        
            string str = stringHelper.GetString(Property);

            if (!string.IsNullOrEmpty(str))
                label = GUIHelper.TempContent(str);

            Vector2 size = Vector2.zero;
            if (label != null)
            {
                size = SirenixGUIStyles.Label.CalcSize(label);
                size.x += 2;
            }
        
            GUIHelper.PushLabelWidth(size.x);
            this.CallNextDrawer(label);
            GUIHelper.PopLabelWidth();
        }
    }
}