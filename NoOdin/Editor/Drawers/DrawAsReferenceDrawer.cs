using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.NoOdin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DrawAsReferenceAttribute))]
internal class DrawAsReferenceDrawer : PropertyDrawer
{
    private FieldInfo _fi;
    private Type[] _typeOptions;

    private FieldInfo[] _pathToHost;
    
    private int _arrayIndex;
    private bool _toggled;

    private float _height;

    private const float _padding = 2;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        _height = lineHeight;
        
        if (_typeOptions == null)
        {
            _pathToHost = property.GetHostInfo(out FieldInfo hostInfo, out _arrayIndex);
            var type = hostInfo.GetReturnType();
            if (type.IsArray)
                type = type.GetElementType();
            _typeOptions = TypeCache.GetTypesDerivedFrom(type).ToArray();
        }

        object target = property.serializedObject.targetObject;
        foreach (var fi in _pathToHost)
            target = fi.GetValue(target);

        if (_arrayIndex >= 0 && target is IList e)
            target = e[_arrayIndex];

        if (target == null)
            DrawTypePicker(position, property, label);
        else
            DrawObjectProperties(position, property, target, label);
    }

    private void DrawObjectProperties(Rect position, SerializedProperty property, object target, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float elementHeight = _padding + lineHeight;

        float originalHeight = position.height;

        position.height = lineHeight;
        label.tooltip = target.GetType().Name;
        _toggled = EditorGUI.Foldout(position, _toggled, label);
        
        EditorGUI.LabelField(position, label.tooltip, CustomGUIStyles.MiniLabelRight);
        
        if (!_toggled) return;

        position = EditorGUI.IndentedRect(position);
        GUIContentHelper.PushIndentLevel(0);

        position.height = originalHeight;
        var linePosition = position.AlignLeft(2).AlignBottom(originalHeight - elementHeight).AddX(-5);
        EditorGUI.DrawRect(linePosition, CustomGUIStyles.DarkEditorBackground);
        position.height = lineHeight;
        
        if (!property.hasChildren)
        {
            position = position.AddY(elementHeight);
            EditorGUI.LabelField(position, "There are no properties to edit.");
            _height += elementHeight;
            return;
        }

        var enumerator = property.GetEnumerator();

        while (enumerator.MoveNext())
        {
            var prop = (SerializedProperty) enumerator.Current;
            position = position.AddY(elementHeight);
            _height += elementHeight;

            EditorGUI.PropertyField(position, prop);
        }
        
        GUIContentHelper.PopIndentLevel();
    }

    private void DrawTypePicker(Rect position, SerializedProperty property, GUIContent label)
    {
        position = EditorGUI.PrefixLabel(position, label);
        
        if (!EditorGUI.DropdownButton(position, new GUIContent("dropdownContent"), FocusType.Passive))
            return;
        
        var menu = new GenericMenu();
        foreach (var type in _typeOptions)
            menu.AddItem(
                new GUIContent(type.Name),
                false, // property.stringValue == type,
                data =>
                {
                    property.managedReferenceValue = (data as Type).CreateInstance();
                    property.serializedObject.ApplyModifiedProperties();
                },
                type);
        menu.DropDown(position);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (_height > 0)
            return _height;
        return base.GetPropertyHeight(property, label);
    }
}