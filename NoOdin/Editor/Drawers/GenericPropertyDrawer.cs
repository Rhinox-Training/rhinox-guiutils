using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.NoOdin.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Base class without generics, try not to inherit from this
/// </summary>
public abstract class GenericPropertyDrawer : PropertyDrawer
{
    protected Dictionary<string, HostInfo> _hostInfoByPath = new Dictionary<string, HostInfo>();
    
    public void SetHostInfo(SerializedProperty property, HostInfo info)
    {
        _hostInfoByPath[property.propertyPath] = info;
    }

    protected HostInfo GetHostInfo(SerializedProperty property)
    {
        var key = property.propertyPath;
        if (_hostInfoByPath.ContainsKey(key))
            return _hostInfoByPath[key];
        return _hostInfoByPath[key] = property.GetHostInfo();
    }

    protected void SetValue(SerializedProperty property, object o)
    {
        property.managedReferenceValue = o;
        property.isExpanded = o != null;
        property.serializedObject.ApplyModifiedProperties();
    }
    
    protected object GetValue(SerializedProperty property)
    {
        return GetHostInfo(property).GetValue();
    }

    public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var id = property.propertyPath;
        var info = GetHostInfo(property);
        if (property.Update(ref info))
        {
            _hostInfoByPath[id] = info;
            OnInitialize(property);
        }
        
        OnPropertyGUI(position, property, label);
        
        EditorGUI.EndProperty();
    }

    protected virtual void OnInitialize(SerializedProperty property)
    {
    }

    protected abstract void OnPropertyGUI(Rect position, SerializedProperty property, GUIContent label);
}

public abstract class GenericPropertyDrawer<T> : GenericPropertyDrawer
    where T : class
{
    protected new T GetValue(SerializedProperty property) => (T) base.GetValue(property);
    protected void SetValue(SerializedProperty property, T value) => base.SetValue(property, value);

}
