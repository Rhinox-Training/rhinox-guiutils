using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Various stuff that only needs to be done once; and not per type instantiated type
    /// Do not inherit from this, inherit from DefaultEditorExtender<>
    /// </summary>
    public abstract class BaseEditorExtender : UnityEditor.Editor
    {
        /// ================================================================================================================
        /// REFLECTION
        // namespace UnityEditor - internal class CustomEditorAttributes
        private static Type _editorAttributesType;
        
        // namespace UnityEditor.CustomEditorAttributes (subclass) - internal readonly struct MonoEditorType
        private static Type _monoEditorTypeType;
#if !UNITY_2023_1_OR_NEWER
        // private static readonly Dictionary<Type, List<MonoEditorType>> kSCustomEditors = new Dictionary<Type, List<MonoEditorType>>();
        private static FieldInfo _customEditorsDictionaryField;
#else
        // private readonly CustomEditorAttributes.CustomEditorCache m_Cache;
        private static FieldInfo _cacheField;
        
        // private static CustomEditorAttributes instance => CustomEditorAttributes.k_Instance.Value;
        private static PropertyInfo _instanceProp;
        
        // internal bool TryGet(Type type, bool multiedition, out List<CustomEditorAttributes.MonoEditorType> editors)
        private static MethodInfo _tryGetEditorsMethod;

#endif
        // internal static void Rebuild()
        private static MethodInfo _rebuildMethod;

        // class MonoEditorType - Type nested under CustomEditorAttributes
        // public Type m_InspectorType;
        private static FieldInfo _inspectorTypeField;
        
        /// ================================================================================================================
        /// FIELDS
        private static IDictionary _editorTypeDictionary;
        
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        private static bool GetEditorsForType(Type type, out Type editorType, out Type editorFallbackType)
        {
            editorType = null;
            editorFallbackType = null;
            
            var typeList = GetEditorsForType(type);
            
            // We need something to work with
            if (typeList == null || typeList.Count == 0)
                return false;
            
            // Ensure our type is in the list
            editorType = GetTypeFromMonoEditorType(typeList[0]);

            // If there is only 1 editor, nothing to do here, the type likely has no default editor
            if (typeList.Count < 2)
                return true;

            // Fetch the default editor type
            editorFallbackType = GetTypeFromMonoEditorType(typeList[1]);
            return true;
        }

        [InitializeOnLoadMethod]
        private static void InitExtendors()
        {
            var editorTypes = TypeCache.GetTypesDerivedFrom(typeof(BaseEditorExtender));

            foreach (var type in editorTypes)
            {
                if (type.ContainsGenericParameters) continue;

                var baseType = ReflectionUtility.GetImplementedGenericType(type, typeof(DefaultEditorExtender<>));
                var unityObjectType = baseType.GetGenericArguments().First();

                bool success = GetEditorsForType(unityObjectType, out var editorType, out var fallbackType);
                if (!success)
                    Debug.LogError($"Failed to initialize {type}. No editors registered?");

                // Check for the CustomEditor attribute
                var customEditorAttribute = AttributeProcessorHelper.FindAttributeInclusive<CustomEditor>(type);
                if (customEditorAttribute == null)
                {
                    Debug.LogError($"Failed to initialize {type}. Did you forget to add [CustomEditor] to your type?");
                    continue;
                }
                
                // Of course this stuff is internal again...
                // if (_customEditorIsInherited == null)
                //     _customEditorIsInherited = typeof(CustomEditor).GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.NonPublic);
                // if (((bool) _customEditorIsInherited.GetValue(customEditorAttribute)) == false)
                // {
                //     // If the editor is not 'inherited' check if we are 
                //     // if type is inherited but our editor is not 'editorForChildClasses', this will throw an error.
                //     // TODO Tested this, it doesn't but for some reason ProbuilderMesh does? INVESTIGATE
                // }

                // Ensure our type is the one we would be drawing
                if (editorType != type)
                {
                    // Debug.LogError($"Failed to initialize {type}. Did you forget to add [CustomEditor] to your type?");
                    continue;
                }

                var initializer = baseType.GetMethod("SetBaseInspectorType", BindingFlags.Static | BindingFlags.NonPublic);
                if (initializer != null)
                    initializer.Invoke(null, new object[] {fallbackType});
            }
        }

        private static Type GetTypeFromMonoEditorType(object defaultEditorTypeContainer)
        {
            if (_inspectorTypeField == null)
            {
                var monoEditorType = defaultEditorTypeContainer.GetType();
#if UNITY_2023_1_OR_NEWER
                _inspectorTypeField = monoEditorType.GetField("inspectorType", BindingFlags.Instance | BindingFlags.Public);
#else
                _inspectorTypeField = monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
#endif
            }

            return _inspectorTypeField.GetValue(defaultEditorTypeContainer) as Type;
        }
        
        private static void FetchEditorAttributesType()
        {
            // Fetch the dictionary of all possible editors for any given type
            // It has a method to return the correct type but that will just return our own editor
            var ass = typeof(UnityEditor.Editor).Assembly;
            _editorAttributesType = ass.GetType("UnityEditor.CustomEditorAttributes");
        }
        
#if !UNITY_2023_1_OR_NEWER
        private static IDictionary GetEditorTypeDictionary()
        {
            // Fetch the dictionary of all possible editors for any given type
            // It has a method to return the correct type but that will just return our own editor
            if (_editorAttributesType == null)
                FetchEditorAttributesType();
            
            if (_customEditorsDictionaryField == null)
                _customEditorsDictionaryField = _editorAttributesType.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.NonPublic);

            var fieldData = _customEditorsDictionaryField.GetValue(null);
            // This is a Dictionary<Type, List<CustomEditorAttributes.MonoEditorType>>
            // The ValueType is, of course, also internal, so... more workarounds
            return fieldData as IDictionary;
        }

        private static IList GetEditorsForType(Type type)
        {
            if (_editorTypeDictionary == null)
            {
                _editorTypeDictionary = GetEditorTypeDictionary();
                // It might not be initialized yet...
                if (_editorTypeDictionary.Count == 0)
                    Rebuild();
            }
            return _editorTypeDictionary[type] as IList;
        }
#else
        private static object GetEditorAttributesInstance()
        {
            if (_editorAttributesType == null)
                FetchEditorAttributesType();
            
            if (_instanceProp == null)
                _instanceProp = _editorAttributesType.GetProperty("instance", BindingFlags.Static | BindingFlags.NonPublic);
            
            return _instanceProp.GetValue(null);
        }
        
        private static IList GetEditorsForType(Type type)
        {
            var instance = GetEditorAttributesInstance();
            if (_cacheField == null)
                _cacheField = _editorAttributesType.GetField("m_Cache", BindingFlags.Instance | BindingFlags.NonPublic);

            var cache = _cacheField.GetValue(instance);

            if (_tryGetEditorsMethod == null)
                _tryGetEditorsMethod = cache.GetType().GetMethod("TryGet", BindingFlags.Instance | BindingFlags.NonPublic);

            if (_monoEditorTypeType == null)
                _monoEditorTypeType = _editorAttributesType.GetNestedType("MonoEditorType", BindingFlags.NonPublic);

            var listType = typeof(List<>).MakeGenericType(_monoEditorTypeType);
            IList list = listType.CreateInstance() as IList;

            var args = new object[] { type, false, list };
            var success = (bool) _tryGetEditorsMethod.Invoke(cache, args);
            return (IList) args[2];
        }
#endif
        
        private static void Rebuild()
        {
            _rebuildMethod = _editorAttributesType.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            _rebuildMethod.Invoke(null, Array.Empty<object>());
        }
    }

// Example for DefaultEditorExtender
/*
[CustomEditor(typeof(MeshRenderer))]
[CanEditMultipleObjects]
public class CustomMeshRendererEditor : DefaultEditorExtender<MeshRenderer>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
            Debug.Log("It works!");
            
    }
}*/

    public abstract class DefaultEditorExtender<T> : BaseEditorExtender where T : UnityEngine.Object
    {
        protected T Target => ConvertObject(target);
        protected T[] Targets => Array.ConvertAll(targets, ConvertObject);

        private static Type _baseEditorType;
        protected UnityEditor.Editor _baseEditor;

        // Called through reflection - Do not change the name
        private static void SetBaseInspectorType(Type editorType)
        {
            _baseEditorType = editorType;

            // Debug.Log($"Test :: {typeof(T)} - {editorType}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // if _baseEditorType == null => There are no custom editors!
            // Not an error though so just make no editor
            if (_baseEditorType == null)
            {
                return;
            }

            // If the 'base' type is equal to this type - abort to prevent StackOverflow errors (Infinitely redrawing itself)
            if (_baseEditorType == GetType())
            {
                Debug.LogError("Something went wrong during initialization of the EditorExtender...");
                return;
            }

            _baseEditor = CreateEditor(targets, _baseEditorType);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_baseEditor)
                DestroyImmediate(_baseEditor);
        }

        public override void OnInspectorGUI()
        {
            if (_baseEditor)
                _baseEditor.OnInspectorGUI();
            else
                base.OnInspectorGUI();
        }

        // Method to prevent lambda alloc
        private T ConvertObject(Object o) => (T) o;
    }
}