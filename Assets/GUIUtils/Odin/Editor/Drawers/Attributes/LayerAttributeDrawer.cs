using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class LayerAttributeDrawer : OdinAttributeDrawer<LayerAttribute, int>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueEntry.SmartValue = EditorGUILayout.LayerField(label, ValueEntry.SmartValue);

            /*SirenixEditorGUI.ErrorMessageBox("The Layer Attribute can only be applied to an integer.", true);
            CallNextDrawer(label);*/
        }
    }
}