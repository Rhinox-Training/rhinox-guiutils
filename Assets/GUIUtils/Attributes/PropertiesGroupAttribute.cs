using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Attributes
{
    public class PropertiesGroupAttribute : PropertyGroupAttribute
    {
        public string RootPropertyName;

        public bool HideWhenDefault;

        public PropertiesGroupAttribute(string rootPropertyName, bool hideWhenDefault = true) : base(rootPropertyName)
        {
            RootPropertyName = rootPropertyName;
            HideWhenDefault = hideWhenDefault;
        }
    }
}