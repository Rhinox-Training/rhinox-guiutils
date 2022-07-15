using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class SerializedObjectExtensions
    {
        private const string _arrayElementExpr = @"([a-zA-Z_]*)\[(\d+)\]";
        private static Regex _arrayElementRegex;

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
                else hostInfo = GetValueInfo(hostInfo, element, subArrayIndex);
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
                    var info = prop.GetHostInfo();
                    return info.GetValue();
                // Represents an array, list, struct or class.
                case SerializedPropertyType.Generic:
                default:
                    throw new NotImplementedException();
            }
            
        }

        private static HostInfo GetValueInfo(SerializedObject root, string element, int arrayIndex = -1)
        {
            var fieldInfo = root.targetObject.GetType().GetField(element, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var hostInfo = new HostInfo(root, fieldInfo, arrayIndex);
            hostInfo.Path = element;
            return hostInfo;
        }
        
        private static HostInfo GetValueInfo(HostInfo host, string element, int arrayIndex = -1)
        {
            var fieldInfo = host.GetReturnType().GetField(element, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return new HostInfo(host, fieldInfo, arrayIndex);
        }
        
        private static HostInfo GetValueInfo(SerializedProperty property)
        {
            return GetValueInfo(property.serializedObject, property.propertyPath);
        }
        
        public static Type GetParentType(this SerializedProperty prop)
        {
            return prop.serializedObject?.targetObject.GetType();
        }
    }
}