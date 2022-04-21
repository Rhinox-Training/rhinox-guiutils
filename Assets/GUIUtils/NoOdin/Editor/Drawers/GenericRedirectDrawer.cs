using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.NoOdin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(DrawAsUnityGenericAttribute), true)]
internal class GenericRedirectDrawer : PropertyDrawer
{
    private struct GenericDrawerInfo
    {
        public Type DrawTargetType;
        public Type PropertyDrawerType;
        public CustomPropertyDrawer Attribute;
        public bool UseForChildClasses;
        public bool DrawerIsGenericTypeDefinition;
    }
    
    private static readonly List<GenericDrawerInfo> _infos = new List<GenericDrawerInfo>();
    private static readonly Dictionary<Type, GenericDrawerInfo> _drawerInfoByTargetType = new Dictionary<Type, GenericDrawerInfo>();

    // =============================================================================================================
    // UnityEditor.CustomPropertyDrawer
    // internal System.Type m_Type;
    private static FieldInfo _typeOfAttributeField;
    // internal bool m_UseForChildren;
    private static FieldInfo _useAttributeForChildrenField;
    
    private FieldInfo _fi;
    private GenericPropertyDrawer _drawer;
    
    [InitializeOnLoadMethod]
    [MenuItem("Rhinox/Reinit GenericRedirectDrawer")]
    private static void InitStatics()
    {
        var propertyDrawers = TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>();
        var infos = propertyDrawers
            .Where(x => x.IsGenericTypeDefinition)
            .Select(x => new GenericDrawerInfo
            {
                PropertyDrawerType = x,
                Attribute = CustomAttributeExtensions.GetCustomAttribute<CustomPropertyDrawer>(x),
                DrawerIsGenericTypeDefinition = x.IsGenericTypeDefinition
            })
            .ToArray();
        
        _typeOfAttributeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
        _useAttributeForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);

        // If one of these fails, unity probably changed their API, update this for your version
        if (_typeOfAttributeField == null || _useAttributeForChildrenField == null)
        {
            Debug.LogError($"Could not initialize {nameof(GenericRedirectDrawer)}...");
            return;
        }
        
        for (var i = 0; i < infos.Length; i++)
        {
            var info = infos[i];
            info.DrawTargetType = (Type) _typeOfAttributeField.GetValue(info.Attribute);

            // If it is not a generic type or it is not picked up by this drawer -> abandon it
            if (!info.DrawTargetType.IsGenericTypeDefinition
                || !info.PropertyDrawerType.InheritsFrom(typeof(GenericPropertyDrawer)))
                continue;
            
            info.UseForChildClasses = (bool) _useAttributeForChildrenField.GetValue(info.Attribute);

            if (info.DrawerIsGenericTypeDefinition)
            {
                _infos.Add(info);
                _drawerInfoByTargetType[info.DrawTargetType] = info;
            }

            if (info.UseForChildClasses)
            {
                var childClasses = TypeCache.GetTypesDerivedFrom(info.DrawTargetType);
                foreach (var c in childClasses)
                {
                    if (!c.IsGenericTypeDefinition)
                    {
                        var types = c.GetArgumentsOfInheritedOpenGenericClass(info.DrawTargetType).ToList();
                        types.Insert(0, c);
                        var typedInfo = info;
                        typedInfo.PropertyDrawerType = typedInfo.PropertyDrawerType.MakeGenericType(types.ToArray());
                        _drawerInfoByTargetType[c] = typedInfo;
                    }
                }
            }
            
            /*
            // TODO limit DrawerIsGenericTypeDefinition to direct implementation
            if (!(info.UseForChildClasses || info.DrawerIsGenericTypeDefinition))
                continue;
            
            foreach (Type targetType in dict.Keys)
            {
                if (!targetType.IsGenericTypeDefinition) continue;

                if (!targetType.InheritsFrom(info.DrawTargetType))
                    continue;
                
                /*var types = targetType.GetArgumentsOfInheritedOpenGenericClass(info.DrawTargetType).ToList();
                types.Insert(0, targetType);
                var drawerType = info.DrawTargetType.MakeGenericType(types.ToArray());
                
                var pair = Activator.CreateInstance(_drawerKeySetType);
                _drawerKeySetDrawerField.SetValue(pair, drawerType);
                _drawerKeySetTypeField.SetValue(pair, targetType);#1#
                
                _drawerTypeByTargetType[targetType] = drawerType;
            }*/
        }
    }

    public override bool CanCacheInspectorGUI(SerializedProperty property)
    {
        if (_drawer != null)
            return _drawer.CanCacheInspectorGUI(property);
        
        return base.CanCacheInspectorGUI(property);
    }
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (_fi == null)
            TryCreateDrawer(property);
        
        if (_drawer != null)
            return _drawer.CreatePropertyGUI(property);
        
        return base.CreatePropertyGUI(property);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_fi == null)
            TryCreateDrawer(property);
        
        if (_drawer != null)
            _drawer.OnGUI(position, property, label);
        else
            base.OnGUI(position, property, label);
    }

    private void TryCreateDrawer(SerializedProperty property)
    {
        var parentType = property.GetParentType();
        _fi = parentType.GetField(property.propertyPath);

        Type drawerType = null;

        if (_drawerInfoByTargetType.TryGetValue(_fi.FieldType, out GenericDrawerInfo drawerInfo))
            drawerType = drawerInfo.PropertyDrawerType;
        else
        {
            var generic = _fi.FieldType.GetGenericTypeDefinition();
            if (_drawerInfoByTargetType.TryGetValue(generic, out drawerInfo))
            {
                var types = _fi.FieldType.GetArgumentsOfInheritedOpenGenericClass(drawerInfo.DrawTargetType).ToList();
                types.Insert(0, _fi.FieldType);
                drawerType = drawerInfo.PropertyDrawerType.MakeGenericType(types.ToArray());
            }
        }

        if (drawerType != null)
        {
            _drawer = (GenericPropertyDrawer) Activator.CreateInstance(drawerType);
            _drawer.SetFieldInfo(_fi);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (_drawer != null)
            return _drawer.GetPropertyHeight(property, label);
        return base.GetPropertyHeight(property, label);
    }
}
