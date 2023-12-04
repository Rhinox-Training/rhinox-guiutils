namespace Rhinox.GUIUtils.Editor
{
    public class TypedHostInfoWrapper<T>
    {
        public GenericHostInfo HostInfo { get;}

        public T SmartValue {
            get => HostInfo.GetSmartValue<T>();
            set => HostInfo.SetValue(value);
        }
        
        public TypedHostInfoWrapper(GenericHostInfo hostInfo)
        {
            HostInfo = hostInfo;
        }

        public TypedHostInfoWrapper<TChild> GetChild<TChild>(int index)
        {
            HostInfo.TryGetChild<TChild>(index, out var typedHostInfo);
            return typedHostInfo;
        }
    }
}