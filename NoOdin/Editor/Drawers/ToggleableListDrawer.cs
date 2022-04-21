using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ToggleableList<>), true)]
public class ToggleableListDrawer<T, TArg> : GenericPropertyDrawer<T>
    where T : ToggleableList<TArg>, new()
{
    private bool _toggled;

    private float _height = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        _height = EditorGUIUtility.singleLineHeight;
        
        EditorGUI.BeginProperty(position, label, property);
        var value = GetValue(property);

        if (value == null)
        {
            value = new T();
            SetValue(property, value);
        }

        position.height = _height;
        GUIContentHelper.PushIndentLevel(0);
        GUI.BeginGroup(position, EditorStyles.toolbar);
        GUI.EndGroup();

        GUIContentHelper.PushHierarchyMode(false);

        _toggled = EditorGUI.Foldout(position, _toggled, label);

        var nPosition = position.AlignRight(50);
        var count = EditorGUI.IntField(nPosition, value.Count, CustomGUIStyles.MiniLabelRight);

        bool changed = count != value.Count;
        
       
        while (count > value.Count)
            value.Add(new Toggleable<TArg>(default, false));
        
        for (int i = value.Count - 1; i > count; --i)
            value.RemoveAt(i);
        
        if (changed)
            property.serializedObject.ApplyModifiedProperties();
        
        // property.Next(true);

        if (_toggled)
        {
            for (int i = 0; i < value.Count; ++i)
            {
                position.AddY(EditorGUIUtility.singleLineHeight);
                if (i < count)
                {
                    // EditorGUILayout.PropertyField(value[i]);
                    _height += EditorGUIUtility.singleLineHeight;
                }
                else
                    value.RemoveAt(i);
            }
        }

        GUIContentHelper.PopHierarchyMode();

        GUIContentHelper.PopIndentLevel();

        EditorGUI.EndProperty();

        const float padding = 2f;
        _height += padding;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (_height > 0)
            return _height;
        return base.GetPropertyHeight(property, label);
    }
}