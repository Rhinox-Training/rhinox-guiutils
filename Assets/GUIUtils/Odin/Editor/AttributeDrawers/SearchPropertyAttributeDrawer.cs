using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin
{
    public class SearchPropertyAttributeDrawer : OdinAttributeDrawer<SearchPropertyAttribute, string>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = GUILayoutUtility.GetRect(GUIHelper.TempContent(ValueEntry.SmartValue), GUIStyle.none);
            ValueEntry.SmartValue = SirenixEditorGUI.SearchField(rect, ValueEntry.SmartValue);

            // if (label != null)
            //     CallNextDrawer(label);
        }
    }
}