using System;
using System.Reflection;
using Rhinox.GUIUtils.NoOdin.Editor;
using UnityEditor;

/// <summary>
/// Base class without generics, try not to inherit from this
/// </summary>
public class GenericPropertyDrawer : PropertyDrawer
{
    protected FieldInfo _fieldInfo;
    
    public void SetFieldInfo(FieldInfo fi)
    {
        _fieldInfo = fi;
    }

    protected void SetValue(SerializedProperty property, object o) => property.managedReferenceValue = o;
    
    protected object GetValue(SerializedProperty property)
    {
        if (_fieldInfo == null)
            _fieldInfo = property.GetParentType().GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return _fieldInfo.GetValue(property.serializedObject.targetObject);
    }
}

public class GenericPropertyDrawer<T> : GenericPropertyDrawer
    where T : class
{
    protected new T GetValue(SerializedProperty property) => (T) base.GetValue(property);
    protected void SetValue(SerializedProperty property, T value) => base.SetValue(property, value);

}
