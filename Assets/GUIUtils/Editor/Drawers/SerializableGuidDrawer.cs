using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(SerializableGuid), true)]
    public class SerializableGuidDrawer : BasePropertyDrawer<SerializableGuid>
    {
        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            var valueRect = EditorGUI.PrefixLabel(position, label);

            using (new eUtility.DisabledGroup())
                GUI.TextField(valueRect, SmartValue.GuidAsString);
            
            var buttonRect = position.AlignRight(25);
            
            if (GUI.Button(buttonRect, GUIContentHelper.TempContent("G", "Regenerate the GUID.")))
                SmartValue.Regenerate();
        }
    }
}