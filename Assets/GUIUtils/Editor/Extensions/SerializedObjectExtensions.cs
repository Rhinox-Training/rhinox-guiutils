using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class SerializedObjectExtensions
    {
        private const string _arrayElementExpr = @"([a-zA-Z_]*)\[(\d+)\]";
        private static Regex _arrayElementRegex;

        private const BindingFlags InstanceFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public static bool Update(this SerializedProperty prop, ref HostInfo info)
        {
            if (info == null || info.Path != prop.propertyPath)
            {
                info = prop.GetHostInfo();
                return true;
            }
            return false;
        }
        
        public static HostInfo GetHostInfo(this SerializedProperty prop)
        {
            if (prop.depth == 0)
            {
                return GetValueInfo(prop);
            }
            
            string path = prop.propertyPath;
            path = path.Replace(".Array.data[", "[");
            string[] parts = path.Split('.');
            
            HostInfo hostInfo = null;

            for (int i = 0; i < parts.Length; ++i)
            {
                string element = parts[i];

                TryMatchArrayElement(ref element, out int subArrayIndex);
                
                if (hostInfo == null)
                    hostInfo = GetValueInfo(prop.serializedObject, element, subArrayIndex);
                else 
                    hostInfo = GetValueInfo(hostInfo, element, subArrayIndex);

                hostInfo.Path = string.Join(".", parts.Take(i));
            }

            hostInfo.Path = prop.propertyPath;
            return hostInfo;
        }

        private static bool TryMatchArrayElement(ref string element, out int index)
        {
            if (_arrayElementRegex == null)
                _arrayElementRegex = new Regex(_arrayElementExpr);

            index = -1;
            
            var match = _arrayElementRegex.Match(element);
            if (!match.Success)
                return false;
            element = match.Groups[1].Value;
            index = int.Parse(match.Groups[2].Value);
            return true;
        }
        
        public static void SetValue(this SerializedProperty property, object value)
        {
            var fi = property.FindFieldInfo();
            fi.SetValue(property.serializedObject.targetObject, value);
        }
        
        public static object GetValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask) prop.intValue;
                case SerializedPropertyType.Enum:
                    return prop.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    return (char)prop.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.FixedBufferSize:
                    return prop.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3Value;
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue;
                // Represents a property that references an object that does not derive from UnityEngine.Object.
                case SerializedPropertyType.ManagedReference:
                case SerializedPropertyType.Generic:// Represents an array, list, struct or class.
                    var info = prop.GetHostInfo();
                    return info.GetValue();
                default:
                    System.Type parentType = prop.serializedObject.targetObject.GetType();
                    System.Reflection.FieldInfo fi = parentType.GetField(prop.propertyPath);
                    return fi.GetValue(prop.serializedObject.targetObject);
            }
            
        }

        public static FieldInfo FindFieldInfo(this SerializedProperty property)
        {
            if (property == null)
                return null;
            
            if (property.propertyPath.Contains(".Array.data[") || property.propertyPath.Contains("."))
            {
                var hostInfo = property.GetHostInfo();
                return (FieldInfo)hostInfo.MemberInfo;
            }
            
            System.Type parentType = property.serializedObject.targetObject.GetType();
            ReflectionUtility.TryGetField(parentType, property.propertyPath, out FieldInfo fi);
            return fi;
        }

        private static HostInfo GetValueInfo(SerializedObject root, string element, int arrayIndex = -1)
        {
            var type = root.targetObject.GetType();
            // Should never be false, throw?
            ReflectionUtility.TryGetField(type, element, out FieldInfo fieldInfo, InstanceFlags);
            var hostInfo = new HostInfo(root, fieldInfo) {
                Path = element
            };
            if (arrayIndex < 0)
                return hostInfo;
            return new HostInfo(hostInfo, fieldInfo, arrayIndex);
        }
        
        private static HostInfo GetValueInfo(HostInfo host, string element, int arrayIndex = -1)
        {
            // Should never be false, throw?
            ReflectionUtility.TryGetField(host.GetReturnType(), element, out FieldInfo fieldInfo, InstanceFlags);
            if (arrayIndex >= 0) // Host becomes the list fi we are dealing with an array element
                host = new HostInfo(host, fieldInfo);
            return new HostInfo(host, fieldInfo, arrayIndex);
        }
        
        private static HostInfo GetValueInfo(SerializedProperty property)
        {
            return GetValueInfo(property.serializedObject, property.propertyPath);
        }
        
        public static Type GetHostType(this SerializedProperty prop)
        {
            return prop.serializedObject?.targetObject.GetType();
        }

        public static T GetAttribute<T>(this SerializedProperty property) where T : Attribute
        {
            System.Reflection.FieldInfo fi = FindFieldInfo(property);
            return fi.GetCustomAttribute(typeof(T)) as T;
        }

        public static IEnumerable<Attribute> GetAttributes(this SerializedProperty property)
        {
            System.Reflection.FieldInfo fi = FindFieldInfo(property);
            return fi.GetCustomAttributes();
        }
        
        public static T GetAttributeOrCreate<T>(this SerializedProperty property) where T : Attribute, new()
        {
            var instance = GetAttribute<T>(property);
            if (instance == null)
                instance = new T();
            return instance;
        }
        
        public struct FieldData
        {
            public object Host;
            public FieldInfo FieldInfo;
            public SerializedProperty SerializedProperty;

            public bool IsSerialized => SerializedProperty != null;
            public bool HoldsUnityObject => FieldInfo.FieldType.InheritsFrom(typeof(UnityEngine.Object));
            public IOrderedDrawable OverrideDrawable;
        }
        
        public static IEnumerable<FieldData> EnumerateEditorVisibleFields(this SerializedProperty property)
        {
            if (property == null)
                yield break;

            var hostInfo = property.GetHostInfo();
            var type = hostInfo.GetReturnType();

            var fields = ReflectionUtility.GetAllFields(type, typeof(UnityEngine.Object));
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<HideInInspector>() != null)
                    continue;
                
                var fieldProperty = property.FindPropertyRelative(field.Name);
                if (fieldProperty == null)
                {
                    if (field.IsPrivate)
                    {
                        var showInInspector = field.GetCustomAttribute<ShowInInspectorAttribute>();
                        if (showInInspector != null)
                        {
                            yield return new FieldData()
                            {
                                Host = property,
                                FieldInfo = field,
                                SerializedProperty = null
                            };
                        }
                    }
                    else
                    {
                        // Unable to serialize
                        // Check support flag
                        if (ShouldDrawUnsupportedWarning(field, out string warning))
                            yield return new FieldData()
                            {
                                Host = property,
                                FieldInfo = field,
                                SerializedProperty = null,
                                OverrideDrawable = new DrawableHelpBox(warning, MessageType.Warning, new HostInfo(hostInfo, field))
                            };
                            
                    }
                    
                    continue;
                }

                yield return new FieldData()
                {
                    Host = property,
                    FieldInfo = field,
                    SerializedProperty = fieldProperty
                };
            }
        }
        
        public static IEnumerable<FieldData> EnumerateEditorVisibleFields(this SerializedObject serializedObject)
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                yield break;
            
            var type = serializedObject.targetObject.GetType();
            var fields = ReflectionUtility.GetAllFields(type, typeof(UnityEngine.Object));
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<HideInInspector>() != null)
                    continue;
                
                var fieldProperty = serializedObject.FindProperty(field.Name);
                if (fieldProperty == null)
                {
                    if (field.IsPrivate)
                    {
                        var showInInspector = field.GetCustomAttribute<ShowInInspectorAttribute>();
                        if (showInInspector != null)
                        {
                            yield return new FieldData()
                            {
                                Host = serializedObject,
                                FieldInfo = field,
                                SerializedProperty = null
                            };
                        }
                    }
                    else
                    {
                        // Unable to serialize
                        // Check support flag
                        if (ShouldDrawUnsupportedWarning(field, out string warning))
                            yield return new FieldData()
                            {
                                Host = serializedObject,
                                FieldInfo = field,
                                SerializedProperty = null,
                                OverrideDrawable = new DrawableHelpBox(warning, MessageType.Warning, new GenericHostInfo(serializedObject.targetObject, field))
                            };
                            
                    }

                    continue;
                }

                yield return new FieldData()
                {
                    Host = serializedObject,
                    FieldInfo = field,
                    SerializedProperty = fieldProperty
                };
            }
        }
        
        private static bool ShouldDrawUnsupportedWarning(FieldInfo fieldInfo, out string warning)
        {
            var attr = fieldInfo.GetReturnType().GetCustomAttribute<UnitySupportWarningAttribute>();
            if (attr == null)
            {
                warning = null;
                return false;
            }

            var currentRuntimeVersion = Utility.GetCurrentUnityRuntime();
            if (attr.Version > currentRuntimeVersion)
            {
                warning = $"This SerializedObject contains a property ({fieldInfo.Name}) of type {fieldInfo.GetReturnType().Name} which is only properly supported from version {attr.VersionString} or higher.";
                return true;
            }

            warning = null;
            return false;
        }
    }
}