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

[CustomPropertyDrawer(typeof(IUnityGenericDrawable), true)]
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
    
    // =============================================================================================================
    // UnityEditor.ScriptAttributeUtility
    private static Type _scriptAttributeUtilityType;
    // private static Dictionary<System.Type, ScriptAttributeUtility.DrawerKeySet> s_DrawerTypeForType;
    private static FieldInfo _drawerTypeForTypeField;
    // private static void BuildDrawerTypeForTypeDictionary()
    private static MethodInfo _buildDrawersMethod;
    
    // private struct DrawerKeySet
    private static Type _drawerKeySetType;
    // public System.Type drawer;
    private static FieldInfo _drawerKeySetDrawerField;
    // public System.Type type;
    private static FieldInfo _drawerKeySetTypeField;
    // =============================================================================================================
    // UnityEditor.CustomPropertyDrawer
    // internal System.Type m_Type;
    private static FieldInfo _typeOfAttributeField;
    // internal bool m_UseForChildren;
    private static FieldInfo _useAttributeForChildrenField;

    private static readonly List<GenericDrawerInfo> _infos = new List<GenericDrawerInfo>();
    private static readonly Dictionary<Type, Type> _drawerTypeByTargetType = new Dictionary<Type, Type>();

    private FieldInfo _fi;
    private GenericPropertyDrawer _drawer;
    
    [InitializeOnLoadMethod]
    [MenuItem("Rhinox/Reinit GenericRedirectDrawer")]
    private static void InitGenericDrawers()
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

        // Reason for fetching these
        // If your drawer is generic, Unity will flip cause it tries to instantiate a version of the generic drawer
        _scriptAttributeUtilityType = typeof(CustomPropertyDrawer).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
        _drawerKeySetType = _scriptAttributeUtilityType.GetNestedType("DrawerKeySet", BindingFlags.NonPublic);
        _drawerKeySetDrawerField = _drawerKeySetType.GetField("drawer");
        _drawerKeySetTypeField = _drawerKeySetType.GetField("type");

        _drawerTypeForTypeField = _scriptAttributeUtilityType.GetField("s_DrawerTypeForType", BindingFlags.NonPublic | BindingFlags.Static);
        _buildDrawersMethod = _scriptAttributeUtilityType.GetMethod("BuildDrawerTypeForTypeDictionary", BindingFlags.NonPublic | BindingFlags.Static);
        
        // Ensure our dict is gonna exist by building it
        _buildDrawersMethod.Invoke(null, Array.Empty<object>());

        _typeOfAttributeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
        _useAttributeForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);
        // Shouldn't be needed but to be sure...
        _infos.Clear();
        _drawerTypeByTargetType.Clear();

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
                || !info.DrawTargetType.InheritsFrom(typeof(IUnityGenericDrawable))
                || !info.PropertyDrawerType.InheritsFrom(typeof(GenericPropertyDrawer)))
                continue;
            
            info.UseForChildClasses = (bool) _useAttributeForChildrenField.GetValue(info.Attribute);

            if (!info.DrawerIsGenericTypeDefinition)
            {
                _infos.Add(info);
                _drawerTypeByTargetType[info.DrawTargetType] = info.PropertyDrawerType;
            }
            
            // TODO limit DrawerIsGenericTypeDefinition to direct implementation
            if (!(info.UseForChildClasses || info.DrawerIsGenericTypeDefinition))
                continue;
            
            var childClasses = TypeCache.GetTypesDerivedFrom(info.DrawTargetType);
            foreach (var c in childClasses)
            {
                if (_drawerTypeByTargetType.ContainsKey(c))
                {
                    Debug.LogWarning($"Found multiple registered editors for {c.AssemblyQualifiedName}.");
                    continue;
                }

                var drawerType = info.PropertyDrawerType;
                if (info.DrawerIsGenericTypeDefinition)
                {
                    var types = c.GetArgumentsOfInheritedOpenGenericClass(info.DrawTargetType).ToList();
                    types.Insert(0, c);
                    drawerType = drawerType.MakeGenericType(types.ToArray());
                }

                RegisterType(c, drawerType);
            }
        }
    }

    private static void RegisterType(Type target, Type drawerType)
    {
        _drawerTypeByTargetType[target] = drawerType;

        var dict = _drawerTypeForTypeField.GetValue(null) as IDictionary;
        
        var pair = Activator.CreateInstance(_drawerKeySetType);
        _drawerKeySetDrawerField.SetValue(pair, drawerType);
        _drawerKeySetTypeField.SetValue(pair, target);
        dict[target] = pair;
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


        if (_drawerTypeByTargetType.TryGetValue(_fi.FieldType, out var drawerType))
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
