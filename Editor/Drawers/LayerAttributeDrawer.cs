using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerAttributeDrawer : PropertyDrawer
    {
        private List<KeyValuePair<int, string>> _layerNameByIndex = new List<KeyValuePair<int, string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.GetReturnType() != typeof(int))
            {
                base.OnGUI(position, property, label);
                return;
            }


            //I recreate the list instead of caching it.
            //If the Tags & Layers list changes (add,remove or rename), then the cache will be stale.
            //Beacuse there is (AFAIK) no direct method when the Tags & Layers names update.
            var options = Utility.GetLayerNames();
            _layerNameByIndex = options.Select(x => new KeyValuePair<int, string>(LayerMask.NameToLayer(x), x)).ToList();

            if (((LayerAttribute)attribute).IsMask)
            {
                //convert the value from full list with empty entries to the null without empty entries
                int convertedMask = 0;
                if (property.intValue != 0)
                {
                    for (int index = 0; index < 32; index++)//there are only 32 layer fields in Unity
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

                //convert the value from the non-null list to the full list with empty entries mask value
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
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            }
        }
    }
}