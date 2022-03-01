using System.Collections;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public static class InspectorPropertyExtensions
    {
        public static InspectorProperty FindSibling(this InspectorProperty prop, string memberName, bool includeSelf = false)
        {
            var parent = prop.Parent ?? prop.SerializationRoot;
            return parent.FindChild(memberName, includeSelf);
        }

        public static InspectorProperty FindChild(this InspectorProperty prop, string memberName, bool includeSelf = false)
        {
            return prop.FindChild(x => FindByName(x, memberName), includeSelf);
        }

        private static bool FindByName(InspectorProperty prop, string name)
        {
            return prop.Name == name;
        }
    }
}