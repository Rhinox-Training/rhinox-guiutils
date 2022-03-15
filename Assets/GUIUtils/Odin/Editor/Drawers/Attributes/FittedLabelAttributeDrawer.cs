using Rhinox.GUIUtils.Odin.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class FittedLabelAttributeDrawer : OdinAttributeDrawer<FittedLabelAttribute>
    {
        private PropertyMemberHelper<string> _stringHelper;
    
        protected override void Initialize()
        {
            _stringHelper = new PropertyMemberHelper<string>(this.Property, this.Attribute.Text);
        }
    
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_stringHelper.ErrorMessage != null)
                SirenixEditorGUI.ErrorMessageBox(_stringHelper.ErrorMessage);
        
            string str = _stringHelper.GetValue();

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