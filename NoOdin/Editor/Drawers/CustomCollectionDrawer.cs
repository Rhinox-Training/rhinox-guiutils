using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(CustomCollection<>), true)]
    public class CustomCollectionDrawer<T, TArg> : GenericPropertyDrawer<T>
        where T : CustomCollection<TArg>, new()
    {
        const float _padding = 2f;

        private ListDrawerSettingsAttribute _settings;

        private Dictionary<string, float> _cachedHeight = new Dictionary<string, float>();

        protected override void OnInitialize(SerializedProperty property)
        {
            _settings = GetHostInfo(property).FieldInfo.GetCustomAttribute<ListDrawerSettingsAttribute>();

            if (_settings != null)
                property.isExpanded = _settings.Expanded;
        }

        protected override void OnPropertyGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;

            string id = property.propertyPath;
            _cachedHeight[id] = lineHeight;

            var value = GetValue(property);

            if (value == null)
            {
                value = new T();
                SetValue(property, value);
            }

            GUIContentHelper.PushIndentLevel(0);
            if (Event.current.type == EventType.Repaint)
                CustomGUIStyles.ToolbarTab.Draw(position, false, false, false, false);
            position.height = lineHeight;

            GUIContentHelper.PushHierarchyMode(false);

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);

            var nPosition = position.AlignRight(28).AddY(_padding).AddX(-_padding * 2);

            if (_settings?.IsReadOnly != true)
            {
                var count = EditorGUI.DelayedIntField(nPosition, value.Count);
                HandleSizeChange(property, count, value);
            }
            else GUI.Label(nPosition, value.Count.ToString());


            if (property.isExpanded)
            {
                // Get all needed properties
                position = position.AddY(_padding);
                _cachedHeight[id] += _padding;

                // var mainProp = property.Copy(); // Main prop -> not needed, handy for checks
                property.Next(true);
                var arrayProp = property.Copy(); // Array prop -> needed to fetch all the elements

                if (!arrayProp.isArray)
                    _cachedHeight[id] += Label(ref position, "Array not properly found, likely a serialization issue.");
                else
                {
                    property.Next(false);
                    var countProp = property.Copy(); // Count prop -> needed to get actual count

                    // Debug.Log($"L: {mainProp.name} | A: {arrayProp.name} | C: {countProp.name}");

                    if (!arrayProp.isArray)
                        _cachedHeight[id] += Label(ref position,
                            "Array not properly found, likely a serialization issue.");
                    else
                        _cachedHeight[id] += HandleItemDrawing(position, arrayProp, countProp.intValue);
                }
            }

            GUIContentHelper.PopHierarchyMode();

            GUIContentHelper.PopIndentLevel();

            _cachedHeight[id] += _padding;
        }

        private static void HandleSizeChange(SerializedProperty property, int count, T value)
        {
            bool changed = count != value.Count;

            while (count > value.Count)
                value.Add(ReflectionUtility.CreateInstance<TArg>());

            for (int i = value.Count - 1; i >= count; --i)
                value.RemoveAt(i);

            if (changed)
                property.serializedObject.ApplyModifiedProperties();
        }

        private float HandleItemDrawing(Rect position, SerializedProperty arrayProp, int size)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float height = 0;
            if (size == 0)
                height += Label(ref position, "There are no items in this collection.");

            for (int i = 0; i < size; ++i)
            {
                var elHeight = _padding + lineHeight;
                var elPosition = position.AddY((i + 1) * elHeight);
                elPosition = elPosition.AlignCenter(elPosition.width - _padding * 2);
                // GUI.BeginGroup(elPosition, EditorStyles.toolbar);
                // GUI.EndGroup();

                var elementProp = arrayProp.GetArrayElementAtIndex(i);
                EditorGUI.PropertyField(elPosition, elementProp, GUIContent.none, false);

                height += elHeight;
            }

            return height;
        }

        private float Label(ref Rect position, string text)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            position = position.AddY(lineHeight);
            GUI.Label(position, text, CustomGUIStyles.MiniLabelLeft);
            return lineHeight;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_cachedHeight.ContainsKey(property.propertyPath))
            {
                var height = _cachedHeight[property.propertyPath];
                if (height > 0)
                    return height + _padding;
            }

            return base.GetPropertyHeight(property, label) + _padding;
        }
    }
}