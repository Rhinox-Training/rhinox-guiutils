using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.IO;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Rhinox.GUIUtils.Editor
{
#if !ODIN_INSPECTOR

    [Serializable]
    internal class SimplePersistentEntry
    {
        public string Path;
        public string Value;
    }
    
    public class SimplePersistentContext<T>
    {
        private T _value;
        private readonly T _defaultVal;
        private readonly string _path;

        public T Value
        {
            set
            {
                _value = value;
                UpdateBackedValue();
            }
            get
            {
                return _value;
            }
        }

        private SimplePersistentContext(string path, T defaultVal)
        {
            _defaultVal = defaultVal;
            _value = _defaultVal;
            _path = path;
            LoadBackedValueIfExists();
        }

        public void SetDefault()
        {
            Value = _defaultVal;
        }

        private void LoadBackedValueIfExists()
        {
            var path = Path.Combine(Application.persistentDataPath, "persistentvalues.json");
            var list = new List<SimplePersistentEntry>();
            if (FileHelper.Exists(path))
            {
                string json = FileHelper.ReadAllText(path);
                list = JsonHelper.FromJson<SimplePersistentEntry>(json).ToList();
                
                // Load value
                var loadedEntry = list.FirstOrDefault(x => x.Path == _path);
                if (loadedEntry != null)
                {
                    byte[] bytes = Convert.FromBase64String(loadedEntry.Value);
                    object obj = ObjectFromByteArray(bytes);
                    _value = obj is T ? (T)obj : default(T);
                }
                
            }
        }
        
        private void UpdateBackedValue()
        {
            var path = Path.Combine(Application.persistentDataPath, "persistentvalues.json");
            var list = new List<SimplePersistentEntry>();
            if (FileHelper.Exists(path))
            {
                string json = FileHelper.ReadAllText(path);
                list = JsonHelper.FromJson<SimplePersistentEntry>(json).ToList();
            }

            var entry = list.FirstOrDefault(x => x.Path == _path);
            if (entry != null)
            {
                entry.Value = Convert.ToBase64String(ObjectToByteArray(_value));
            }
            else
                list.Add(new SimplePersistentEntry()
                {
                    Path = _path,
                    Value = Convert.ToBase64String(ObjectToByteArray(_value))
                });
            
            string resultJson = JsonHelper.ToJson(list.ToArray(), true);
            File.WriteAllText(path, resultJson);
        }
        
        private static byte[] ObjectToByteArray(System.Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        private static System.Object ObjectFromByteArray(byte[] arr)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream(arr))
            {
                var obj = bf.Deserialize(ms);
                return obj;
            }
        }

        public static SimplePersistentContext<T> Create<TKey1, TKey2>(TKey1 key1, TKey2 key2,
            T defaultVal = default(T))
        {
            string path = $"{SmartToString(key1)}//{SmartToString(key2)}";
            var context = new SimplePersistentContext<T>(path, defaultVal);
            return context;
        }
        
        public static SimplePersistentContext<T> Create<TKey1, TKey2, TKey3>(TKey1 key1, TKey2 key2, TKey3 key3,
            T defaultVal = default(T))
        {
            string path = $"{SmartToString(key1)}//{SmartToString(key2)}";
            var context = new SimplePersistentContext<T>(path, defaultVal);
            return context;
        }

        private static string SmartToString<TKey>(TKey key)
        {
            if (key == null)
                return "<NULL>";
            if (key is Type typeKey)
            {
                return typeKey.FullName;
            }

            return key.ToString();
        }
    }
#endif
    
    // ================================================================================================================
    // PERSISTENT VALUE
    public class PersistentValue<T>
    {
#if ODIN_INSPECTOR
        private GlobalPersistentContext<T> _context;
#else
        private SimplePersistentContext<T> _context;
#endif
        
        private PersistentValue() { }

        public bool Set(T value)
        {
            if (Equals(value, _context.Value)) return false;
            _context.Value = value;
            return true;
        }

        public static implicit operator T(PersistentValue<T> value)
        {
            if (value?._context == null) 
                return default;
            return value._context.Value;
        }

        public static PersistentValue<T> Create<TKey>(TKey key, T defaultValue, [CallerMemberName] string memberKey = null)
        {
            return new PersistentValue<T>
            {
#if ODIN_INSPECTOR
                _context = PersistentContext.Get(key, memberKey, defaultValue)
#else
                _context = SimplePersistentContext<T>.Create(key, memberKey, defaultValue)
#endif
            };
        }

        // ================================================================================================================
        // CREATE KEYS
        public static PersistentValue<T> Create<TKey1, TKey2>(TKey1 key1, TKey2 key2, T defaultValue)
        {
            return new PersistentValue<T>
            {
#if ODIN_INSPECTOR
                _context = PersistentContext.Get(key1, key2, defaultValue)
#else
                _context = SimplePersistentContext<T>.Create(key1, key2, defaultValue)
#endif
            };
        }

        public static PersistentValue<T> Create<TKey1, TKey2, TKey3>(TKey1 key1, TKey2 key2, TKey3 key3, T defaultValue)
        {
            return new PersistentValue<T>
            {
#if ODIN_INSPECTOR
                _context = PersistentContext.Get(key1, key2, key3, defaultValue)
#else
                _context = SimplePersistentContext<T>.Create(key1, key2, key3, defaultValue)
#endif
            };
        }
    }
}