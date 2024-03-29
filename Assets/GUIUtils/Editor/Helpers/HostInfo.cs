using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
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

        public override ICollection<Attribute> GetAttributes()
        {
            return AttributeProcessorHelper.FindAllAttributesInclusive(GetReturnType());
        }
    }

    public class ParameterHostInfo : GenericHostInfo
    {
        private ParameterInfo _info;
        
        public ParameterHostInfo(GenericHostInfo parent, ParameterInfo pi, int arrayIndex)
            : base(parent, null, arrayIndex)
        {
            _info = pi;
            NiceName = _info.GetNiceName();
        }

        public override Type GetReturnType(bool preferValueType = true)
        {
            return _info.ParameterType;
        }

        public override ICollection<Attribute> GetAttributes()
        {
            var typeAttr = AttributeProcessorHelper.FindAllAttributesInclusive(GetReturnType());
            var directAttr = _info.GetCustomAttributes(); // ParameterInfo does not have injectable attributes
            return directAttr.Concat(typeAttr).ToArray();
        }

        protected override void OnValueChanged(object host)
        { }
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
            EditorUtility.SetDirty(Root.targetObject);
            // Update should handle what base.OnValueChanged does
            // base.OnValueChanged(host);
        }

        public override void Apply()
        {
            Root.ApplyModifiedProperties();
            base.Apply();
        }

        protected override GenericHostInfo CreateChildHostInfo(MemberInfo member, int index = -1)
        {
            if (member is FieldInfo fieldInfo)
                return new HostInfo(this, fieldInfo, index);
            return base.CreateChildHostInfo(member, index);
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

        public string NiceName { get; protected set; }

        public GenericHostInfo(object host, MemberInfo mi, int index = -1)
            : this(host, mi, index, null)
        {
        }
        
        public GenericHostInfo(GenericHostInfo parent, object host, MemberInfo mi, int index = -1)
            : this(host, mi, index, parent)
        {
        }

        public GenericHostInfo(GenericHostInfo parent, MemberInfo mi, int index = -1)
            : this((object) null, mi, index, parent)
        {
        }
        
        public GenericHostInfo(GenericHostInfo parent, MemberInfo mi, Type type, int index = -1)
            : this(type, mi, index, parent)
        {
        }
        
        private GenericHostInfo(object host, MemberInfo memberInfo, int arrayIndex, GenericHostInfo parent)
            : this(null, memberInfo, arrayIndex, parent)
        {
            if (parent != null)
                HostType = parent.GetReturnType();
            else if (host != null)
                HostType = host.GetType();
            else
                throw new ArgumentException($"{nameof(parent)} and {nameof(host)} cannot be null at the same time");
            
            _hostRootInstance = host;
        }
        
        private GenericHostInfo(Type type, MemberInfo memberInfo, int arrayIndex, GenericHostInfo parent)
        {
            HostType = type;
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
            if (host is ICollection collection) // TODO: should we?
            {
                int index = 0;
                foreach (var entry in collection)
                {
                    if (ArrayIndex == index++)
                        return entry;
                }
            }
            throw new IndexOutOfRangeException($"Could not map found index {ArrayIndex} to value {host} (Type: {host.GetType().GetNiceName()})");
        }

        public T GetSmartValue<T>() => (T) GetValue();

        public void SetValue(object obj)
        {
            TrySetValue(obj);
        }

        public bool CanSetValue()
        {
            if (MemberInfo is PropertyInfo propertyInfo)
                return propertyInfo.GetSetMethod(true) != null;
            return true;
        }

        public virtual bool TrySetValue(object val)
        {
            if (!CanSetValue())
                return false;
            
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
                        MemberInfo.SetValue(Parent.GetHost(), e);
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

        public void ForceNotifyValueChanged()
        {
            var host = GetHost();
            OnValueChanged(host);
        }

        protected virtual void OnValueChanged(object host)
        {
            if (Parent == null)
                return;
            
            if (HostType.IsValueType)
                Parent.TrySetValue(host);
            else
                Parent.OnValueChanged(Parent.GetHost());
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
            
            if (MemberInfo == null)
                return HostType;

            Type type = MemberInfo.GetReturnType();

            var typeHintAttr = AttributeProcessorHelper.FindAttributeInclusive<HostInfoTypeHintAttribute>(MemberInfo, HostType);
            if (typeHintAttr != null)
            {
                var hintType = TryGetHintType(GetHost(), typeHintAttr.Member);
                if (hintType != null)
                    type = hintType;
            }
            
            if (ArrayIndex < 0) return type;
            if (type.IsArray)
                return type.GetElementType();
            return type.GetArgumentsOfInheritedOpenGenericClass(typeof(IList<>)).First();
        }

        private static Type TryGetHintType(object instance, string member)
        {
            var memberHelper = MemberHelper.Create<Type>(instance, member);
            if (!memberHelper.HasError)
                return memberHelper.ForceGetValue();
            
            var altMemberHelper = MemberHelper.Create<SerializableType>(instance, member);
            if (altMemberHelper.HasError)
                return null;
            var hintType = altMemberHelper.ForceGetValue();
            if (hintType == null)
                return null;
            return hintType;
        }

        public bool TryGetAttribute<T>(out T attribute) where T : Attribute
        {
            attribute = GetAttribute<T>();
            return attribute != null;
        }
        
        public T GetAttribute<T>() where T : Attribute
        {
            return GetAttributes().OfType<T>().FirstOrDefault();
        }
        
        public virtual ICollection<Attribute> GetAttributes()
        {
            var typeAttr = AttributeProcessorHelper.FindAllAttributesInclusive(GetReturnType());
            if (ArrayIndex != -1)
                return typeAttr;
            
            var directAttr = AttributeProcessorHelper.FindAllAttributesInclusive(MemberInfo, HostType);
            return directAttr.Concat(typeAttr).ToArray();
        }

        public override string ToString()
        {
            return $"{_hostRootInstance}.{NiceName} {(Parent != null ? ($"(Child of {Parent.NiceName})") : "")}";
        }

        public virtual GenericHostInfo CreateArrayElement(int index, Type overrideType = null)
        {
            if (index < 0) throw new ArgumentException(nameof(index));
            if (ArrayIndex != -1) throw new InvalidOperationException("GenericHostInfo already has in index, cannot create sub entry.");
            
            if (overrideType != null)
                return new GenericHostInfo(this, MemberInfo, overrideType, index);

            return new GenericHostInfo(this, MemberInfo, index);
        }

        public virtual void Apply()
        {
            
        }

        public bool TryGetChild(string name, out GenericHostInfo childHostInfo, int index = -1)
        {
            Type t = GetReturnType();
            if (t == null)
            {
                childHostInfo = null;
                return false;
            }
            
            var members = t.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Default | BindingFlags.NonPublic);
            if (members.Length > 1 || members.Length == 0)
            {
                childHostInfo = null;
                return false;
            }

            if (index != -1)
            {
                var collectionHostInfo = CreateChildHostInfo(members[0]);
                childHostInfo = collectionHostInfo.CreateChildHostInfo(members[0], index);
                return true;
            }

            childHostInfo = CreateChildHostInfo(members[0]);
            return true;
        }

        public bool TryGetChild<T>(string name, out TypedHostInfoWrapper<T> childHostInfo)
        {
            if (TryGetChild(name, out var info))
            {
                childHostInfo = new TypedHostInfoWrapper<T>(info);
                return true;
            }

            childHostInfo = null;
            return false;
        }

        public bool TryGetChild<T>(int index, out TypedHostInfoWrapper<T> childHostInfoWrapper)
        {
            if (ArrayIndex != -1)
            {
                childHostInfoWrapper = null;
                return false;
            }
            
            var childHostInfo = CreateChildHostInfo(MemberInfo, index);
            childHostInfoWrapper = new TypedHostInfoWrapper<T>(childHostInfo);
            return true;
            
        }

        protected virtual GenericHostInfo CreateChildHostInfo(MemberInfo member, int index = -1)
        {
            return new GenericHostInfo(this, member, index);
        }

        public string GetNicePath()
        {
            string path = ArrayIndex == -1 ? $"{NiceName}" : $"[{ArrayIndex.ToString()}]";

            var currentHostInfo = Parent;
            GenericHostInfo previousHostInfo = this;
            while (currentHostInfo != null)
            {
                var suffixPath = previousHostInfo.ArrayIndex != -1 ? path : $".{path}";
                
                path = currentHostInfo.ArrayIndex == -1 ?
                    $"{currentHostInfo.NiceName}{suffixPath}" :
                    $"[{currentHostInfo.ArrayIndex}]{suffixPath}";

                previousHostInfo = currentHostInfo;
                currentHostInfo = currentHostInfo.Parent;
            }

            return path;
        }
    }
}