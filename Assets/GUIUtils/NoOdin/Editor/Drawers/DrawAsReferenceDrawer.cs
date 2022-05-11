using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

    private HostInfo _info;

    private bool _toggled;
    
    private const float _padding = 2;

    private GUIContent _noneContent = new GUIContent("None");
    private Dictionary<string, GUIContent> _typeContentByName = new Dictionary<string, GUIContent>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.Update(ref _info))
        {
            var type = _info.GetReturnType(false);
            _typeOptions = TypeCache.GetTypesDerivedFrom(type).ToArray();
        }
        
        Rect dropdownPosition = position.AlignTop(EditorGUIUtility.singleLineHeight);

        DrawTypePicker(dropdownPosition, property, label);
        
        EditorGUI.PropertyField(position, property, GUIContent.none, true);
        
        EditorGUI.EndProperty();
    }

    private SerializedProperty _property;
    private void DrawTypePicker(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_info.ArrayIndex >= 0)
        {
            label = GUIContentHelper.TempContent($"{_info.ArrayIndex}: ");
            position = EditorGUI.IndentedRect(position);
            GUI.Label(position, label);
            position = position.AlignRight(position.width - 30);
        }
        else
            position = EditorGUI.PrefixLabel(position, label);

        if (!EditorGUI.DropdownButton(position, GetTypeName(property), FocusType.Passive))
            return;

        var menu = new GenericMenu();
        menu.AddItem(_noneContent, false, SetManagedReference, null);
        foreach (var type in _typeOptions)
            menu.AddItem(
                new GUIContent(type.Name),
                false,
                SetManagedReference,
                type);
        _property = property;
        menu.DropDown(position);
    }

    private void SetManagedReference(object data)
    {
        var value = (data as Type).CreateInstance();
        _property.managedReferenceValue = value;
        _property.isExpanded = value != null;
        _property.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property,true);
    }
    
    GUIContent GetTypeName(SerializedProperty property)
    {
        // Cache this string.
        string fullTypeName = property.managedReferenceFullTypename;

        if (string.IsNullOrEmpty(fullTypeName))
            return _noneContent;

        if (_typeContentByName.TryGetValue(fullTypeName, out var cachedTypeName))
            return cachedTypeName;

        Type type = _info.GetReturnType();
        
        if (type == null)
            return _noneContent;
        
        string typeName = GetTypeCSharpRepresentation(type);

        GUIContent result = new GUIContent(typeName);
        _typeContentByName[fullTypeName] = result;
        return result;
    }

    public static string GetTypeCSharpRepresentation(Type type, Stack<Type> genericArgs = null, StringBuilder arrayBrackets = null)
    {
        StringBuilder code = new StringBuilder();
        Type declaringType = type.DeclaringType;

        bool arrayBracketsWasNull = arrayBrackets == null;

        if (genericArgs == null)
            genericArgs = new Stack<Type>(type.GetGenericArguments());


        int currentTypeGenericArgsCount = genericArgs.Count;
        if (declaringType != null)
            currentTypeGenericArgsCount -= declaringType.GetGenericArguments().Length;

        Type[] currentTypeGenericArgs = new Type[currentTypeGenericArgsCount];
        for (int i = currentTypeGenericArgsCount - 1; i >= 0; i--)
            currentTypeGenericArgs[i] = genericArgs.Pop();


        if (declaringType != null)
            code.Append(GetTypeCSharpRepresentation(declaringType, genericArgs)).Append('.');


        if (type.IsArray)
        {
            if (arrayBrackets == null)
                arrayBrackets = new StringBuilder();

            arrayBrackets.Append('[');
            arrayBrackets.Append(',', type.GetArrayRank() - 1);
            arrayBrackets.Append(']');

            Type elementType = type.GetElementType();
            code.Insert(0, GetTypeCSharpRepresentation(elementType, arrayBrackets: arrayBrackets));
        }
        else
        {
            code.Append(new string(type.Name.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_').ToArray()));

            if (currentTypeGenericArgsCount > 0)
            {
                code.Append('<');
                for (int i = 0; i < currentTypeGenericArgsCount; i++)
                {
                    code.Append(GetTypeCSharpRepresentation(currentTypeGenericArgs[i]));
                    if (i < currentTypeGenericArgsCount - 1)
                        code.Append(',');
                }

                code.Append('>');
            }

            if (declaringType == null && !string.IsNullOrEmpty(type.Namespace))
            {
                code.Insert(0, '.').Insert(0, type.Namespace);
            }
        }


        if (arrayBracketsWasNull && arrayBrackets != null)
            code.Append(arrayBrackets.ToString());


        return code.ToString();
    }
}