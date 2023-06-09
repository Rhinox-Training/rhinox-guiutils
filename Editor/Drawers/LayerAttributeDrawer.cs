using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using Rhinox.GUIUtils.Utils;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEditor.UIElements;
using Rhinox.Lightspeed;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer : PropertyDrawer
    {
        //private Dictionary<int, string> _layerNameByIndex;
        private List<KeyValuePair<int, string>> _layerNameByIndex = new List<KeyValuePair<int, string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.GetReturnType() != typeof(int))
            {
                base.OnGUI(position, property, label);
                return;
            }

            var options = LayerUtils.GetLayerNames();
            _layerNameByIndex = options.Select(x => new KeyValuePair<int, string>(LayerMask.NameToLayer(x), x)).ToList();

            if (((LayerAttribute)attribute).IsMask)
            {
                int convertedMask = 0;
                if (property.intValue != 0)
                {
                    for (int index = 0; index < 32; index++)
                    {
                        if ((property.intValue >> index & 1) == 1)
                        {
                            convertedMask |= 1 << _layerNameByIndex.FindIndex(x => x.Key == index);

                            property.intValue -= 1 << index;
                            if (property.intValue == 0)
                                break;
                        }
                    }
                }

                convertedMask = EditorGUI.MaskField(position, label, convertedMask, options);

                property.intValue = 0;
                if (convertedMask != 0)
                {
                    for (int index = 0; index < _layerNameByIndex.Count; index++)
                    {
                        if ((convertedMask >> index & 1) == 1)
                        {
                            property.intValue |= 1 << _layerNameByIndex[index].Key;

                            convertedMask -= 1 << index;
                            if (convertedMask == 0)
                                break;
                        }
                    }
                }
            }
            else
            {
                var result = _layerNameByIndex.FirstOrDefault(x => x.Key == property.intValue);
                string dropDownText;
                if (!result.IsDefault())
                {
                    dropDownText = result.Value;
                }
                else
                {
                    dropDownText = "<Select a value>";
                    property.intValue = -1;
                }

                EditorGUILayout.BeginHorizontal();
                var controlRect = EditorGUI.PrefixLabel(position, label);
                if (EditorGUI.DropdownButton(controlRect, GUIContentHelper.TempContent(dropDownText), FocusType.Passive, EditorStyles.miniPullDown))
                {
                    var menu = new GenericMenu();

                    foreach (var option in options)
                    {
                        menu.AddItem(new GUIContent(option), false, () =>
                        {
                            property.intValue = LayerMask.NameToLayer(option);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                    menu.DropDown(controlRect);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}