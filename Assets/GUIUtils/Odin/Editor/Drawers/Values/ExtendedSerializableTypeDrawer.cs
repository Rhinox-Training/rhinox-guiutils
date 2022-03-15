#if ODIN_INSPECTOR
using System.Linq;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0, 0, 2004)]
    public class ExtendedSerializableTypeDrawer : SerializableTypeDrawer
    {
        protected override void Initialize()
        {
            base.Initialize();

            var assignableTypeFilter = Property.Attributes.OfType<AssignableTypeFilterAttribute>().FirstOrDefault();
            if (assignableTypeFilter != null)
                _baseType = assignableTypeFilter.BaseType;
        }
    }
}
#endif
