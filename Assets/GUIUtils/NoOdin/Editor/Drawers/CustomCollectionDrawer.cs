using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CustomCollection<>), true)]
public class CustomCollectionDrawer<T, TArg> : GenericPropertyDrawer<T>
    where T : CustomCollection<TArg>, new()
{
    private bool _toggled;

    private float _height = -1;
    
    const float _padding = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        _height = lineHeight;
        
        EditorGUI.BeginProperty(position, label, property);
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

        _toggled = EditorGUI.Foldout(position, _toggled, label);

        var nPosition = position.AlignRight(28).AddY(_padding).AddX(-_padding*2);
        var count = EditorGUI.DelayedIntField(nPosition, value.Count);

        bool changed = count != value.Count;
        
        while (count > value.Count)
            value.Add(ReflectionUtility.CreateInstance<TArg>());
        
        for (int i = value.Count - 1; i >= count; --i)
            value.RemoveAt(i);
        
        if (changed)
            property.serializedObject.ApplyModifiedProperties();
        
        if (_toggled)
        {
            // Get all needed properites
            position = position.AddY(_padding);
            // var mainProp = property.Copy(); // Main prop -> not needed, handy for checks
            property.Next(true);
            var arrayProp = property.Copy(); // Array prop -> needed to fetch all the elements
            
            if (!arrayProp.isArray)
                position = Label(position, "Array not properly found, likely a serialization issue.");
            else
            {
                property.Next(false);
                var countProp = property.Copy(); // Count prop -> needed to get actual count
            
                // Debug.Log($"L: {mainProp.name} | A: {arrayProp.name} | C: {countProp.name}");
            
                if (!arrayProp.isArray)
                    position = Label(position, "Array not properly found, likely a serialization issue.");
                else
                    HandleItemDrawing(position, arrayProp, countProp.intValue);
            }
        }

        GUIContentHelper.PopHierarchyMode();

        GUIContentHelper.PopIndentLevel();

        EditorGUI.EndProperty();
        
        _height += _padding;
    }

    private void HandleItemDrawing(Rect position, SerializedProperty arrayProp, int size)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;

        if (size == 0)
            position = Label(position, "There are no items in this collection.");

        for (int i = 0; i < size; ++i)
        {
            var elHeight = _padding + lineHeight;
            var elPosition = position.AddY((i + 1) * elHeight);
            elPosition = elPosition.AlignCenter(elPosition.width - _padding * 2);
            // GUI.BeginGroup(elPosition, EditorStyles.toolbar);
            // GUI.EndGroup();

            var elementProp = arrayProp.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(elPosition, elementProp, GUIContent.none, false);

            _height += elHeight;
        }
    }

    private Rect Label(Rect position, string text)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        _height += lineHeight;
        position = position.AddY(lineHeight);
        GUI.Label(position, text, CustomGUIStyles.MiniLabelLeft);
        return position;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (_height > 0)
            return _height + _padding;
        return base.GetPropertyHeight(property, label) + _padding;
    }
}