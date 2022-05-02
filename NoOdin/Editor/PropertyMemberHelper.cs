using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    public static class SerializedObjectExtensions
    {
        private const string _arrayElementExpr = @"([a-zA-Z_]*)\[(\d+)\]";
        private static Regex _arrayElementRegex;
        
        public static FieldInfo[] GetHostInfo(this SerializedProperty prop, out FieldInfo hostInfo, out int arrayIndex)
        {
            var target = prop.serializedObject.targetObject;
            arrayIndex = -1;
            Type type = target.GetType();

            if (prop.depth == 0)
            {
                hostInfo = GetValueInfo(type, prop.propertyPath);
                return new[] { hostInfo };
            }
            
            var infos = new List<FieldInfo>();
            string element;
            FieldInfo targetInfo;
            
            string path = prop.propertyPath;
            path = path.Replace(".Array.data[", "[");
            string[] parts = path.Split('.');
            for (int i = 0; i < parts.Length - 1; ++i)
            {
                element = parts[i];

                TryMatchArrayElement(ref element, out int subArrayIndex);
                targetInfo = GetValueInfo(type, element);
                type = targetInfo.GetReturnType();
                infos.Add(targetInfo);
            }

            element = parts[parts.Length - 1];
            TryMatchArrayElement(ref element, out arrayIndex);
            hostInfo = GetValueInfo(type, element);
            infos.Add(hostInfo);

            return infos.ToArray();
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
                    var target = prop.serializedObject.targetObject;
                    var info = GetValueInfo(target.GetType(), prop.propertyPath);
                    return info.GetValue(prop.serializedObject.targetObject);
                // Represents an array, list, struct or class.
                case SerializedPropertyType.Generic:
                default:
                    throw new NotImplementedException();
            }
            
        }

        private static FieldInfo GetValueInfo(Type type, string element)
        {
            return type.GetField(element, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        public static Type GetHostType(this SerializedProperty prop)
        {
            GetHostInfo(prop, out FieldInfo hostInfo, out int i);
            if (hostInfo == null) return prop.GetParentType();
            
            var type = hostInfo.GetReturnType();
            return i < 0 ? type : type.GetElementType();
        }
        
        public static Type GetParentType(this SerializedProperty prop)
        {
            return prop.serializedObject?.targetObject.GetType();
        }
    }
    
    /// <summary>
    /// VERY BASIC VERSION FOR WHEN NO ACCESS TO ODIN
    /// Helper class to handle strings for labels and other similar purposes.
    /// Allows for a static string, or for referring to string member fields, properties or methods, by name
    /// </summary>
    public class PropertyMemberHelper<T>
    {
        private T _cachedValue;
        private string _errorMessage;
        private readonly Type _objectType;
        
        private Func<T> _staticValueGetter;
        private Func<object, T> _instanceValueGetter;
        
        private UnityEngine.Object _propertyHost;

        /// <summary>
        /// If any error occurred while looking for members, it will be stored here.
        /// </summary>
        public string ErrorMessage => _errorMessage;

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        public PropertyMemberHelper(SerializedProperty property, string text)
            : this(property.serializedObject == null, text, property)
        {
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="property">Inspector property to get string from.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        public PropertyMemberHelper(SerializedProperty property, string text, ref string errorMessage)
            : this(property, text)
        {
            if (errorMessage != null)
                return;
            errorMessage = this.ErrorMessage;
        }

        /// <summary>Creates a StringMemberHelper to get a display string.</summary>
        /// <param name="objectType">The type of the parent, to get a member string from.</param>
        /// <param name="isStatic">Value indicating if the context should be static.</param>
        /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
        /// /// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
        private PropertyMemberHelper(bool isStatic, string text, SerializedProperty property)
        {
            _propertyHost = property.serializedObject.targetObject;
            _objectType = property.GetParentType();
            
            if (string.IsNullOrEmpty(text) || _objectType == null || text.Length <= 0)
                return;
            
            if (text[0] == '@')
            {
                this._errorMessage = "Expressions are only supported with Odin Enabled";
                return;
            }

            const string PARENT_ID = "parent";
            const string ROOT_ID = "root";

            if (text[0] == '$')
            {
                text = text.Substring(1);
                int partI = -1;
                while ((partI = text.IndexOf(".", StringComparison.Ordinal)) >= 0)
                {
                    var part = text.Substring(0, partI);
                    bool actionTaken = true;
                    switch (part)
                    {
                        case PARENT_ID:
                            _errorMessage = $"${part} is only supported with Odin Enabled";
                            break;
                        case ROOT_ID:
                            _errorMessage = $"${part} is only supported with Odin Enabled";
                            break;
                        default:
                            actionTaken = false;
                            break;
                    }

                    if (!actionTaken)
                        break;
                }
            }
            
            // property might have changed
            _objectType = property.GetParentType();

            var flags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = _objectType.FindMembers(
                    MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    flags, 
                    (mi, crit) => mi.GetReturnType() == typeof(T), 
                    null
            );

            var member = members.FirstOrDefault();

            switch (member)
            {
                case MethodInfo mi:
                    if (mi.IsStatic())
                        this._staticValueGetter = () => (T) mi.Invoke(null, new object[] {});
                    else
                        this._instanceValueGetter = (i) => (T) mi.Invoke(i, new object[] {});
                    break;
                case FieldInfo fi:
                    if (fi.IsStatic())
                        this._staticValueGetter = () => (T) fi.GetValue(null);
                    else
                        this._instanceValueGetter = (i) => (T) fi.GetValue(i);
                    break;
                case PropertyInfo pi:
                    if (pi.IsStatic())
                        this._staticValueGetter = () => (T) pi.GetValue(null);
                    else
                        this._instanceValueGetter = (i) => (T) pi.GetValue(i);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Gets a value indicating whether or not the string is retrieved from a from a member.
        /// </summary>
        public bool IsDynamicString => this._instanceValueGetter != null
                                       || this._staticValueGetter != null;

        /// <summary>Gets the type of the object.</summary>
        public Type ObjectType => this._objectType;
        
        /// <summary>
        /// Gets the value from the MemberHelper.
        /// </summary>
        public T GetValue()
        {
            if (this._cachedValue == null || Event.current == null || Event.current.type == EventType.Layout)
                this._cachedValue = this.ForceGetValue(_propertyHost);
            return this._cachedValue;
        }

        public T GetValue(ref bool changed)
        {
            var oldValue = _cachedValue;
            var newValue = GetValue();
            if (!Equals(oldValue, newValue))
                changed = true;
            
            return newValue;
        }

        /// <summary>Forcefully fetches a new value, ignoring any caches.</summary>
        public T ForceGetValue(object instance)
        {
            if (this._errorMessage != null)
                return default;

            if (this._staticValueGetter != null)
                return this._staticValueGetter();

            if (_instanceValueGetter != null)
                return _instanceValueGetter(instance);
            
            return default;
        }
    }
}