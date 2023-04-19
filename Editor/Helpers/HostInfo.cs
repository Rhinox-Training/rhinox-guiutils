using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public class HostInfo : GenericHostInfo
    {
        public readonly HostInfo Parent;
        public string Path;
        private SerializedObject _hostSerializedObject;
        
        public SerializedObject Root
        {
            get => Parent == null ? _hostSerializedObject : Parent.Root;
            set
            {
                if (Parent != null) Parent.Root = value;
                else _hostSerializedObject = value;
            }
        }
        
        public HostInfo(SerializedObject host, FieldInfo fi, int index = -1)
            : base(host.targetObject, fi, index)
        {
            _hostSerializedObject = host;
            Path = null;
        }

        public HostInfo(HostInfo parent, FieldInfo fi, int index = -1)
            : base(null, fi, index)
        {
            Parent = parent;
            Path = null;
        }

        public override object GetHost()
        {
            if (Parent == null)
                return base.GetHost();
            return Parent.GetValue();
        }
        
        protected override void BeforeValueChanged()
        {
            base.BeforeValueChanged();
            //_root.ApplyModifiedProperties(); // TODO: do we need ApplyModifiedProperties here?
        }

        protected override void OnValueChanged()
        {
            Root.Update();
            base.OnValueChanged();
        }

    }
    
    public class GenericHostInfo
    {
        public readonly FieldInfo FieldInfo;
        public readonly int ArrayIndex;
        
        private readonly object _hostRootInstance;
        
        private static MethodInfo _resizeMethod;

        public string NiceName => FieldInfo?.Name.SplitCamelCase();

        public GenericHostInfo(object host, FieldInfo fi, int index = -1)
        {
            _hostRootInstance = host;
            FieldInfo = fi;
            ArrayIndex = index;
        }

        public virtual object GetHost()
        {
            return _hostRootInstance;
        }

        public virtual object GetValue()
        {
            var value = FieldInfo.GetValue(GetHost());
            if (ArrayIndex < 0) return value;
            if (value is IList e)
                return e[ArrayIndex];
            throw new IndexOutOfRangeException($"Could not map found index {ArrayIndex} to value {value}");
        }

        public void SetValue(object obj)
        {
            if (ArrayIndex < 0)
            {
                BeforeValueChanged();
                FieldInfo.SetValue(GetHost(), obj);
                OnValueChanged();
                return;
            }

            var value = FieldInfo.GetValue(GetHost());
            if (value is IList e)
            {
                BeforeValueChanged();
                if (ArrayIndex >= e.Count)
                {
                    if (e is Array eArr)
                    {
                        object arr = eArr;
                        ResizeArray(ref arr, ArrayIndex + 1);
                        e = (IList)arr;
                        e[ArrayIndex] = obj;
                        FieldInfo.SetValue(GetHost(), e);
                    }
                    else
                        e.Insert(ArrayIndex, obj);
                }
                else
                    e[ArrayIndex] = obj;

                OnValueChanged();
                return;
            }

            throw new IndexOutOfRangeException($"Could not map found index {ArrayIndex} to value {value}");
        }

        protected virtual void BeforeValueChanged()
        {
            
        }

        protected virtual void OnValueChanged()
        {
            
        }

        private static void ResizeArray(ref object array, int n)
        {
            var type = array.GetType();
            var elemType = type.GetElementType();
            if (_resizeMethod == null)
                _resizeMethod = typeof(Array).GetMethod("Resize", BindingFlags.Static | BindingFlags.Public);
            if (_resizeMethod == null)
                return;
            var properResizeMethod = _resizeMethod.MakeGenericMethod(elemType);
            var parameters = new object[] { array, n };
            properResizeMethod.Invoke(null, parameters);
            array = parameters[0];
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