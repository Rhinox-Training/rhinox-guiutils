using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Rhinox.GUIUtils.Editor
{
    public static class ScriptEditorHeaderIcons
    {
        private static Assembly _editorAssembly;

        private static Assembly EditorAssembly =>
            _editorAssembly ?? (_editorAssembly = Assembly.GetAssembly(typeof(EditorWindow)));

        private static Type _attributeType;
        private static Type _helperType;
        private static Type _helperMiSorterType;
        private static Type _helperMethodWithAttributeType;

        private static FieldInfo _helperDictionaryField;   
        private static FieldInfo _miSorterBackingListField;

        private static MethodInfo _helperDictionaryInitMethod;
        
        private static FieldInfo _helperStructMethodInfo;
        private static FieldInfo _helperStructAttribute;

        private static bool? _initialized;

        private static bool Init()
        {
            if (_initialized.HasValue)
                return _initialized.Value;

            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
            
            // internal sealed class EditorHeaderItemAttribute { public System.Type TargetType; }
            _attributeType = eUtility.EditorAssembly.GetType("UnityEditor.EditorHeaderItemAttribute");
            
            _helperType = eUtility.EditorAssembly.GetType("UnityEditor.AttributeHelper");
            if (_helperType == null)
            {
                Debug.LogWarning("ScriptEditorHeaderIcons.Init Failed.");
                _initialized = false;
                return false;
            }
            _helperDictionaryInitMethod = _helperType
                .GetMethod("GetMethodsWithAttribute", flags)
                ?.MakeGenericMethod(_attributeType);
            
            // internal class MethodInfoSorter { public IEnumerable<MethodWithAttribute> methodsWithAttributes { get; } }
            _helperMiSorterType = _helperType.GetNestedType("MethodInfoSorter", flags);
            _miSorterBackingListField = _helperMiSorterType.GetField("<methodsWithAttributes>k__BackingField", flags);
            
            // Dictionary<System.Type, AttributeHelper.MethodInfoSorter> s_DecoratedMethodsByAttrTypeCache
            _helperDictionaryField = _helperType.GetField("s_DecoratedMethodsByAttrTypeCache", flags);
            
            // internal struct MethodWithAttribute { public MethodInfo info; public Attribute attribute; }
            _helperMethodWithAttributeType = _helperType.GetNestedType("MethodWithAttribute", flags);
            _helperStructMethodInfo = _helperMethodWithAttributeType.GetField("info");
            _helperStructAttribute = _helperMethodWithAttributeType.GetField("attribute");

            _initialized = true;
            return true;
        }

        public static void RegisterMethod(MethodInfo info)
        {
            if (!Init())
                return;
            
            var dict = (IDictionary) _helperDictionaryField.GetValue(null);
            if (!dict.Contains(_attributeType))
                _helperDictionaryInitMethod.Invoke(null, new object[]
                {
                    BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                });
            var infoSorter = dict[_attributeType];
            var list = (IList) _miSorterBackingListField.GetValue(infoSorter);

            var attr = Activator.CreateInstance(_attributeType, typeof(Object), -999);

            var value = _helperMethodWithAttributeType.CreateInstance();
            _helperStructMethodInfo.SetValue(value, info);
            _helperStructAttribute.SetValue(value, attr);
            list.Add(value);
        }

    }
}