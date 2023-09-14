namespace Rhinox.GUIUtils.Editor
{
    public class TypedHostInfoWrapper<T>
    {
        public GenericHostInfo HostInfo { get;}

        public T SmartValue {
            get { return HostInfo.GetSmartValue<T>(); }
            set { HostInfo.SetValue(value); }
        }
        
        public TypedHostInfoWrapper(GenericHostInfo hostInfo)
        {
            HostInfo = hostInfo;
        }
    }
}