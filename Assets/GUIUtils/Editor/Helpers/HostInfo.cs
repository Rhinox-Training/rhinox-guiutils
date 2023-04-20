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
        public readonly GenericHostInfo Parent;
        public readonly MemberInfo MemberInfo;
        public readonly int ArrayIndex;
        
        private readonly object _hostRootInstance;
        
        private static MethodInfo _resizeMethod;

        public string NiceName => MemberInfo?.Name.SplitCamelCase();

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
            if (memberInfo is MethodBase) throw new ArgumentException(nameof(memberInfo));
            if (parent == null && host == null) throw new ArgumentException($"{nameof(parent)} and {nameof(host)} cannot be null at the same time");
            _hostRootInstance = host;
            MemberInfo = memberInfo;
            ArrayIndex = arrayIndex;
            Parent = parent;
        }

        public virtual object GetHost()
        {
            if (Parent == null)
                return _hostRootInstance;
            return Parent.GetValue();
        }

        public virtual object GetValue()
        {
            if (ArrayIndex < 0)
            {
                var value = MemberInfo.GetValue(GetHost());
                return value;
            }

            var hostList = GetHost();
            if (hostList is IList e)
                return e[ArrayIndex];
            throw new IndexOutOfRangeException($"Could not map found index {ArrayIndex} to value {hostList} (Type: {hostList.GetType().GetNiceName()})");
        }

        public T GetSmartValue<T>() => (T) GetValue();

        public void SetValue(object obj)
        {
            TrySetValue(obj);
        }

        public virtual bool TrySetValue(object val)
        {
            if (ArrayIndex < 0)
            {
                BeforeValueChanged();
                MemberInfo.SetValue(GetHost(), val);
                OnValueChanged();
                return true;
            }

            var value = MemberInfo.GetValue(GetHost());
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
                        e[ArrayIndex] = val;
                        MemberInfo.SetValue(GetHost(), e);
                    }
                    else
                        e.Insert(ArrayIndex, val);
                }
                else
                    e[ArrayIndex] = val;

                OnValueChanged();
                return true;
            }

            return false;
        }

        protected virtual void BeforeValueChanged()
        {
            
        }

        protected virtual void OnValueChanged()
        {
            
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

        public Type GetReturnType(bool preferValueType = true)
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
        
        public Type GetHostType() => MemberInfo.DeclaringType;

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
    }
}