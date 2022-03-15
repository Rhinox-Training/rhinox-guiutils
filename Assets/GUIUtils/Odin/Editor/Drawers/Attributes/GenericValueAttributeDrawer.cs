using System;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0, 0, 2500)]
    public class GenericValueAttributeDrawer : OdinAttributeDrawer<GenericValueAttribute>
    {
        private Type _type;
        private string _cachedTypeName;

        private PropertyMemberHelper<string> _typeHelper;
        private string _errorMessage;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            if (Attribute.TargetType != null)
                _type = Attribute.TargetType;
            else
            {
                _type = typeof(object);
                _typeHelper = new PropertyMemberHelper<string>(Property, Attribute.TypeName);
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            
            bool showMixedValue = EditorGUI.showMixedValue;
            
            if (Property.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
                EditorGUI.showMixedValue = true;

            const bool allowSceneObjects = true;
            var type = GetTargetType();
            object val = Property.ValueEntry.WeakSmartValue;
            if (type.InheritsFrom(typeof(UnityEngine.Object)))
                val = SirenixEditorFields.UnityObjectField(label, val as Object, type, allowSceneObjects);
            else
                val = SirenixEditorFields.PolymorphicObjectField(label, val, type, allowSceneObjects);
            
            EditorGUI.showMixedValue = showMixedValue;
            
            if (!EditorGUI.EndChangeCheck())
                return;
            
            Property.Tree.DelayActionUntilRepaint(() =>
            {
                Property.ValueEntry.WeakValues[0] = val;
                for (int index = 1; index < Property.ValueEntry.ValueCount; ++index)
                    Property.ValueEntry.WeakValues[index] = SerializationUtility.CreateCopy(val);
            });
        }

        private Type GetTargetType()
        {
            if (_typeHelper == null)
                return _type;
            
            var typeName = _typeHelper.GetValue();
            if (typeName == _cachedTypeName)
                return _type;
            
            _cachedTypeName = typeName;
            _type = ReflectionUtility.FindTypeExtensively(ref typeName);
            return _type;
        }
    }
}