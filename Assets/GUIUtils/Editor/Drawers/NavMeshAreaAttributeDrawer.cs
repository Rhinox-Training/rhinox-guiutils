using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute))]
    public class NavMeshLayerAttributeDrawer : PropertyDrawer
    {
        private Dictionary<int, string> _areaNameByIndex;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.GetReturnType() != typeof(int))
            {
                base.OnGUI(position, property, label);
                return;
            }

            if (((NavMeshAreaAttribute) attribute).IsMask)
            {
                var options = GameObjectUtility.GetNavMeshAreaNames();
                property.intValue = EditorGUI.MaskField(position, label, property.intValue, options);
            }
            else
            {
                var options = GameObjectUtility.GetNavMeshAreaNames();
                if (_areaNameByIndex == null || options.Any(x => !_areaNameByIndex.ContainsValue(x)))
                {
                    _areaNameByIndex = new Dictionary<int, string>();
                    foreach (var option in options)
                    {
                        _areaNameByIndex.Add(NavMesh.GetAreaFromName(option), option);
                    }
                }

                if (!_areaNameByIndex.ContainsKey(property.intValue))
                    property.intValue = -1;
                
                if (EditorGUI.DropdownButton(position, GUIContentHelper.TempContent(property.intValue == -1 ? "<Select a value>" : _areaNameByIndex[property.intValue]), FocusType.Passive, EditorStyles.miniPullDown))
                {
                    var menu = new GenericMenu();
                    
                    foreach (var option in options)
                    {
                        menu.AddItem(new GUIContent(option), false, () =>
                        {
                            property.intValue = NavMesh.GetAreaFromName(option);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                    menu.DropDown(position);
                }
            }
        }
    }
}