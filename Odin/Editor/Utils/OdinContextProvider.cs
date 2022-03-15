using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class OdinContextProvider
    {
        protected LocalPersistentContext<T> GetPersistentValue<T>(string key, T defaultValue = default(T))
        {
            GlobalPersistentContext<T> context;
            if (PersistentContext.Get(TwoWaySerializationBinder.Default.BindToName(GetType()), key, out context))
                context.Value = defaultValue;
            return LocalPersistentContext<T>.Create(context);
        }
    }

    public static class OdinPersistentContextHelper
    {
        public static LocalPersistentContext<T> GetPersistentValue<T>(this OdinEditor editor, string key,
            T defaultValue = default(T))
        {
            GlobalPersistentContext<T> context;
            if (PersistentContext.Get(TwoWaySerializationBinder.Default.BindToName(editor.GetType()), key, out context))
                context.Value = defaultValue;
            return LocalPersistentContext<T>.Create(context);
        }
    }
}