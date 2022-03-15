using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0.0, 0.0, 3500.0)]
    public class DrawAsObjectPickerAttributeDrawer<T> : OdinAttributeDrawer<DrawAsUnityObjectAttribute, T> where T : Object
    {
        private bool allowSceneObjects;

        protected override void Initialize()
        {
            allowSceneObjects = ValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueEntry.SmartValue = (T) SirenixEditorFields.UnityObjectField(label, ValueEntry.SmartValue, typeof(T), allowSceneObjects);
        }
    }
}