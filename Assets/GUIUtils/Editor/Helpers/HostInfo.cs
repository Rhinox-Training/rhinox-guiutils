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
    public sealed class RootHostInfo : GenericHostInfo
    {
        public RootHostInfo(object host) : base(host, null)
        {
        }
        
        public override object GetValue() => GetHost();
        public override Type GetReturnType(bool preferValueType = true) => HostType;
        public override Attribute[] GetAttributes() => Array.Empty<Attribute>();
    }
    
    public class MethodHostInfo : GenericHostInfo
    {
        private ParameterInfo[] _parameters;
        public MethodHostInfo(GenericHostInfo parent, MethodInfo mi)
            : base(parent, mi)
        { }
        
        public MethodHostInfo(object instance, MethodInfo mi)
            : base(instance, mi)
        { }
        

        public void Invoke(params object[] parameters)
        {
            MemberInfo?.GetValue(GetHost(), parameters);
        }

        public ParameterInfo[] GetParameters()
        {
            if (_parameters == null)
            {
                var mi = (MemberInfo as MethodInfo);
                _parameters = mi.GetParameters();
            }
            
            return _parameters;
        }
    }
    
    public class HostInfo : GenericHostInfo
    {
        public string Path;
        private SerializedObject _hostSerializedObject;
        
        public new HostInfo Parent => (HostInfo) base.Parent;
        
        public SerializedObject Root
        {
            get => Parent == null ? _hostSerializedObject : Parent.Root;
            set
            {
                if (Parent != null) 
                    Parent.Root = value;
                else 
                    _hostSerializedObject = value;
            }
        }
        
        public HostInfo(SerializedObject host, FieldInfo mi, int index = -1)
            : base(host.targetObject, mi, index)
        {
            _hostSerializedObject = host;
            Path = null;
        }

        public HostInfo(HostInfo parent, FieldInfo mi, int index = -1)
            : base(parent, mi, index)
        {
            Path = null;
        }

        protected override void OnValueChanged(object host)
        {
            Root.Update();
            // Update should handle what base.OnValueChanged does
            // base.OnValueChanged(host);
        }

        public override void Apply()
        {
            Root.ApplyModifiedProperties();
            base.Apply();
        }
    }
    
    public class GenericHostInfo
    {
        public readonly GenericHostInfo Parent;
        public readonly Type HostType;
        public readonly MemberInfo MemberInfo;
        public readonly int ArrayIndex;
        
        private readonly object _hostRootInstance;
        
        private static MethodInfo _resizeMethod;

        public string NiceName { get; }

        public GenericHostInfo(object host, MemberInfo mi, int index = -1)
            : this(host, mi, index, null)
        {
        }
        
        public GenericHostInfo(GenericHostInfo parent, object host, MemberInfo mi, int index = -1)
            : this(host, mi, index, parent)
        {
        }

        public GenericHostInfo(GenericHostInfo parent, MemberInfo mi, int index = -1)
            : this(null, mi, index, parent)
        {
        }

        private GenericHostInfo(object host, MemberInfo memberInfo, int arrayIndex, GenericHostInfo parent)
        {
            if (parent != null)
                HostType = parent.GetReturnType();
            else if (host != null)
                HostType = host.GetType();
            else
                throw new ArgumentException($"{nameof(parent)} and {nameof(host)} cannot be null at the same time");

            _hostRootInstance = host;
            MemberInfo = memberInfo;
            ArrayIndex = arrayIndex;
            Parent = parent;
            NiceName = MemberInfo?.GetNiceName();
        }

        public virtual object GetHost()
        {
            if (Parent == null)
            {
                // If we are a list element, we still need to actually fetch the list to get our host
                // Since you cannot have a fieldinfo of an element, the fieldinfo must be of our list
                if (ArrayIndex >= 0)
                    return MemberInfo.GetValue(_hostRootInstance);
                // If we are not, we can trust that our FieldInfo points to us and the root instance is therefore, our host
                return _hostRootInstance;
            }
            return Parent.GetValue();
        }

        public virtual object GetValue()
        {
            var host = GetHost();
            
            if (host == null)
                throw new ArgumentException($"Cannot resolve host.");
            
            // If we are not a list element, we need to fetch the value from our host
            if (ArrayIndex < 0)
                return MemberInfo.GetValue(host);
            
            // If we are, then we received a list and can access it by our index
            if (host is IList list)
                return list[ArrayIndex];
            throw new IndexOutOfRangeException($"Could not map found index {ArrayIndex} to value {host} (Type: {host.GetType().GetNiceName()})");
        }

        public T GetSmartValue<T>() => (T) GetValue();

        public void SetValue(object obj)
        {
            TrySetValue(obj);
        }

        public virtual bool TrySetValue(object val)
        {
            var host = GetHost();

            if (ArrayIndex < 0)
            {
                MemberInfo.SetValue(host, val);
                OnValueChanged(host);
                return true;
            }

            if (host is IList e)
            {
                if (ArrayIndex >= e.Count)
                {
                    if (e is Array eArr)
                    {
                        object arr = eArr;
                        ResizeArray(ref arr, ArrayIndex + 1);
                        e = (IList)arr;
                        e[ArrayIndex] = val;
                        MemberInfo.SetValue(GetHost(), e);
                    }
                    else
                        e.Insert(ArrayIndex, val);
                }
                else
                    e[ArrayIndex] = val;

                OnValueChanged(host);
                return true;
            }

            return false;
        }

        protected virtual void OnValueChanged(object host)
        {
            if (HostType.IsValueType && Parent != null)
                Parent.TrySetValue(host);
        }

        // TODO: Do we migrate this to Rhinox.Lightspeed?
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

        public virtual Type GetReturnType(bool preferValueType = true)
        {
            if (preferValueType)
            {
                var value = GetValue();
                if (value != null)
                    return value.GetType();
            }
            
            var type = MemberInfo.GetReturnType();
            if (ArrayIndex < 0) return type;
            if (type.IsArray)
                return type.GetElementType();
            return type.GetArgumentsOfInheritedOpenGenericClass(typeof(IList<>)).First();
        }
        
        public T GetAttribute<T>() where T : Attribute
        {
            return GetAttributes().OfType<T>().FirstOrDefault();
        }
        
        public virtual Attribute[] GetAttributes()
        {
            var typeAttr = GetReturnType().GetCustomAttributes();
            if (ArrayIndex != -1)
                return typeAttr;
            
            var directAttr = MemberInfo.GetCustomAttributes();
            return directAttr.Concat(typeAttr).ToArray();
        }

        public override string ToString()
        {
            return $"{_hostRootInstance}.{NiceName} {(Parent != null ? ($"(Child of {Parent.NiceName})") : "")}";
        }

        public virtual GenericHostInfo CreateArrayElement(int index)
        {
            if (index < 0) throw new ArgumentException(nameof(index));
            if (ArrayIndex != -1) throw new InvalidOperationException("GenericHostInfo already has in index, cannot create sub entry.");

            return new GenericHostInfo(this, MemberInfo, index);
        }

        public virtual void Apply()
        {
            
        }
    }
}