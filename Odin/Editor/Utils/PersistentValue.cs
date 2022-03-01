using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.GUIUtils.Odin.Editor
{
    // ================================================================================================================
    // PERSISTENT VALUE
    public class PersistentValue<T>
    {
        private GlobalPersistentContext<T> _context;

        private PersistentValue()
        {
        }

        public void Set(T value)
        {
            if (Equals(value, _context.Value)) return;
            _context.Value = value;
        }

        public static implicit operator T(PersistentValue<T> value)
        {
            if (value?._context == null) return default;
            return value._context.Value;
        }

        public static PersistentValue<T> Create<TKey>(TKey key, T defaultValue, [CallerMemberName] string memberKey = null)
        {
            return new PersistentValue<T>
            {
                _context = PersistentContext.Get(key, memberKey, defaultValue)
            };
        }

        // ================================================================================================================
        // CREATE KEYS
        public static PersistentValue<T> Create<TKey1, TKey2>(TKey1 key1, TKey2 key2, T defaultValue)
        {
            return new PersistentValue<T>
            {
                _context = PersistentContext.Get(key1, key2, defaultValue)
            };
        }

        public static PersistentValue<T> Create<TKey1, TKey2, TKey3>(TKey1 key1, TKey2 key2, TKey3 key3, T defaultValue)
        {
            return new PersistentValue<T>
            {
                _context = PersistentContext.Get(key1, key2, key3, defaultValue)
            };
        }
    }
}