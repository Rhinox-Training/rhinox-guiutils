using System;
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
        
        private HostInfo _info;

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
            _objectType = property.GetParentType();
            
            if (string.IsNullOrEmpty(text) || _objectType == null || text.Length <= 0)
                return;
            
            if (text[0] == '@')
            {
                this._errorMessage = "Expressions are only supported with Odin Enabled";
                return;
            }
            
            _info = property.GetHostInfo();

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
                            _info = _info.Parent;
                            break;
                        case ROOT_ID:
                            while (_info.Parent != null)
                                _info = _info.Parent;
                            break;
                        default:
                            actionTaken = false;
                            break;
                    }

                    if (actionTaken)
                        text = text.Substring(part.Length+1);
                    else
                        break;
                }
            }
            
            // property might have changed
            _objectType = _info.GetHostType();

            var flags = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            flags |= BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var members = _objectType.FindMembers(
                    MemberTypes.Property | MemberTypes.Field | MemberTypes.Method,
                    flags, 
                    (info, crit) => info.GetReturnType() == typeof(T) && info.Name == text, 
                    null
            );

            var mi = members.FirstOrDefault();
            
            if (mi == null)
                _errorMessage = $"Could not find field {text} on type {_objectType.Name}";
            else if (mi.IsStatic())
                this._staticValueGetter = () => (T) mi.GetValue(null);
            else
                this._instanceValueGetter = (i) => (T) mi.GetValue(i);
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
            if (IsNewFrame())
            {
                this._cachedValue = this.ForceGetValue(_info?.GetHost());
            }
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

        private bool _isNewFrame, _nextEventIsNew;
        private bool IsNewFrame()
        {
            if (Event.current == null)
                return _isNewFrame;
            EventType type = Event.current.type;
            if (type == EventType.Repaint)
            {
                _nextEventIsNew = true;
                _isNewFrame = false;
                return _isNewFrame;
            }
            
            if (_nextEventIsNew)
            {
                _nextEventIsNew = false;
                _isNewFrame = true;
                return _isNewFrame;
            }
            _isNewFrame = false;
            return _isNewFrame;
        }
    }
}