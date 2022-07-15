using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class HostInfo
    {
        private SerializedObject _root;
        public SerializedObject Root
        {
            get => Parent == null ? _root : Parent.Root;
            set
            {
                if (Parent != null) Parent.Root = value;
                else _root = value;
            }
        }
        public readonly HostInfo Parent;
        public readonly FieldInfo FieldInfo;
        public readonly int ArrayIndex;
        public string Path;

        public HostInfo(SerializedObject obj, FieldInfo fi, int index = -1)
        {
            _root = obj;
            FieldInfo = fi;
            ArrayIndex = index;
            Parent = null;
        }

        public HostInfo(HostInfo parent, FieldInfo fi, int index = -1)
        {
            Parent = parent;
            FieldInfo = fi;
            ArrayIndex = index;
        }

        public object GetHost()
        {
            if (Parent == null)
                return _root.targetObject;
            return Parent.GetValue();
        }

        public object GetValue()
        {
            var value = FieldInfo.GetValue(GetHost());
            if (ArrayIndex < 0) return value;
            if (value is IList e)
                return e[ArrayIndex];
            throw new IndexOutOfRangeException($"Could not map found index {ArrayIndex} to value {value}");
        }

        public Type GetReturnType(bool preferValueType = true)
        {
            if (preferValueType)
            {
                var value = GetValue();
                if (value != null)
                    return value.GetType();
            }
            
            var type = FieldInfo.GetReturnType();
            if (ArrayIndex < 0) return type;
            if (type.IsArray)
                return type.GetElementType();
            return type.GetArgumentsOfInheritedOpenGenericClass(typeof(IList<>)).First();
        }
        
        public Type GetHostType() => FieldInfo.DeclaringType;
    }
}