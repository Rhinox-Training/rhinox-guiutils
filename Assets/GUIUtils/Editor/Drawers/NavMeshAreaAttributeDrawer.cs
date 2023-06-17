using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute))]
    public class NavMeshLayerAttributeDrawer : PropertyDrawer
    {
        private List<KeyValuePair<int, string>> _areaNameByIndex = new List<KeyValuePair<int, string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.GetReturnType() != typeof(int))
            {
                base.OnGUI(position, property, label);
                return;
            }

            //I recreate the list instead of caching it.
            //If the navmesh area list changes (add,remove or rename), then the cache will be stale.
            //Beacuse there is (AFAIK) no direct method when the Navmesh area names update.
            var options = GameObjectUtility.GetNavMeshAreaNames();
            _areaNameByIndex = options.Select(x => new KeyValuePair<int, string>(NavMesh.GetAreaFromName(x), x)).ToList();

            if (((NavMeshAreaAttribute)attribute).IsMask)
            {
                int convertedMask = 0;
                if (property.intValue != 0)
                {
                    for (int index = 0; index < 32; index++)
                    {
                        if ((property.intValue >> index & 1) == 1)
                        {
                            convertedMask |= 1 << _areaNameByIndex.FindIndex(x => x.Key == index);

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
                    for (int index = 0; index < _areaNameByIndex.Count; index++)
                    {
                        if ((convertedMask >> index & 1) == 1)
                        {
                            property.intValue |= 1 << _areaNameByIndex[index].Key;

                            convertedMask -= 1 << index;
                            if (convertedMask == 0)
                                break;
                        }
                    }
                }
            }
            else
            {
                var result = _areaNameByIndex.FirstOrDefault(x => x.Key == property.intValue);
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
                        menu.AddItem(new GUIContent(option), dropDownText.Equals(option), () =>
                        {
                            property.intValue = NavMesh.GetAreaFromName(option);
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