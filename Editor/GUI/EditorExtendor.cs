using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
#endif

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Various stuff that only needs to be done once; and not per type instantiated type
    /// </summary>
    public abstract class BaseEditorExtender
#if ODIN_INSPECTOR
        : OdinEditor
#else
        : UnityEditor.Editor
#endif
    {
        // namespace UnityEditor - internal class CustomEditorAttributes
        private static Type _editorAttributesType;

        // private static readonly Dictionary<Type, List<MonoEditorType>> kSCustomEditors = new Dictionary<Type, List<MonoEditorType>>();
        private static FieldInfo _customEditorsDictionaryField;

        // internal static void Rebuild()
        private static MethodInfo _rebuildMethod;

        // class MonoEditorType - Type nested under CustomEditorAttributes
        // public Type m_InspectorType;
        private static FieldInfo _inspectorTypeField;

#if !ODIN_INSPECTOR // OdinEditor implements these, to allow easy override make stubs
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
#endif

        [InitializeOnLoadMethod]
        private static void InitExtendors()
        {
            if (_editorAttributesType != null) return;

            var dictionary = GetEditorTypeDictionary();
            // It might not be initialized yet...
            if (dictionary.Count == 0)
                Rebuild();

            var editorTypes = TypeCache.GetTypesDerivedFrom(typeof(BaseEditorExtender));
            foreach (var type in editorTypes)
            {
                if (type.ContainsGenericParameters) continue;

                var baseType = ReflectionUtility.GetImplementedGenericType(type, typeof(DefaultEditorExtender<>));
                var unityObjectType = baseType.GetGenericArguments().First();
                var typeList = dictionary[unityObjectType] as IList;

                // We need something to work with
                if (typeList == null || typeList.Count == 0)
                {
                    Debug.LogError($"Failed to initialize {type}. No editors registered.");
                    continue;
                }

                // Ensure our type is in the list
                var probablyOurType = GetTypeFromMonoEditorType(typeList[0]);
                if (probablyOurType != type)
                {
                    Debug.LogError($"Failed to initialize {type}. Did you forget to add [CustomEditor] to your type?");
                    continue;
                }

                // If there is only 1 editor, nothing to do here, the type likely has no default editor
                if (typeList.Count < 2)
                    continue;


                // Fetch the default editor type
                var defaultEditorType = ExtractDefaultEditorType(typeList);

                var initializer = baseType.GetMethod("SetBaseInspectorType", BindingFlags.Static | BindingFlags.NonPublic);
                if (initializer != null)
                    initializer.Invoke(null, new object[] {defaultEditorType});
            }
        }

        private static Type ExtractDefaultEditorType(IList typeList)
        {
            // typeList = List<MonoEditorType>; this is a class nested under CustomEditorAttributes
            // The first item would be our custom editor
            var defaultEditorTypeContainer = typeList[1];
            return GetTypeFromMonoEditorType(defaultEditorTypeContainer);
        }

        private static Type GetTypeFromMonoEditorType(object defaultEditorTypeContainer)
        {
            if (_inspectorTypeField == null)
            {
                var monoEditorType = defaultEditorTypeContainer.GetType();
                _inspectorTypeField = monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
            }

            return _inspectorTypeField.GetValue(defaultEditorTypeContainer) as Type;
        }

        private static IDictionary GetEditorTypeDictionary()
        {
            // Fetch the dictionary of all possible editors for any given type
            // It has a method to return the correct type but that will just return our own editor
            if (_editorAttributesType == null)
            {
                var ass = typeof(UnityEditor.Editor).Assembly;
                _editorAttributesType = ass.GetType("UnityEditor.CustomEditorAttributes");
            }
            
            if (_customEditorsDictionaryField == null)
                _customEditorsDictionaryField = _editorAttributesType.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.NonPublic);

            var fieldData = _customEditorsDictionaryField.GetValue(null);
            // This is a Dictionary<Type, List<CustomEditorAttributes.MonoEditorType>>
            // The ValueType is, of course, also internal, so... more workarounds
            return fieldData as IDictionary;
        }

        private static void Rebuild()
        {
            _rebuildMethod = _editorAttributesType.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            _rebuildMethod.Invoke(null, new object[] { });
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
}
*/

    public abstract class DefaultEditorExtender<T> : BaseEditorExtender where T : UnityEngine.Object
    {
        private static Type _baseEditorType;
        protected UnityEditor.Editor _baseEditor;

        protected T Target => target as T;
        protected T[] Targets => Array.ConvertAll(targets, ConvertObject);

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