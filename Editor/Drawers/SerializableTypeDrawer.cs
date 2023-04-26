using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : ValuePropertyDrawer<SerializableType>
    {
        // Yeah it's ugly but eh
        public class Null
        {
        }

        private string _typeMethod;
        private string _title;
        private Type _baseType;

        private IPropertyMemberHelper _rawGetter;
        private PickerHandler _typePicker;

        private static List<Type> _allTypes;

        protected override void Initialize()
        {
            base.Initialize();
            
            if (fieldInfo.TryGetAttribute(out AssignableTypeFilterAttribute assignableTypeFilter))
            {
                _baseType = assignableTypeFilter.BaseType;
                _title = assignableTypeFilter.DropdownTitle;
            }
            else if (fieldInfo.TryGetAttribute(out TypeFilterAttribute typeFilter))
            {
                _typeMethod = typeFilter.MemberName;
                _title = typeFilter.DropdownTitle;

                _rawGetter = MemberHelper.Create<object>(this.Property, _typeMethod);
            }
        }

        private IEnumerable<Type> ResolveRawGetter()
        {
            var result = _rawGetter.GetValue();
            if (result is IEnumerable list)
            {
                foreach (var o in list)
                {
                    switch (o)
                    {
                        case Type t:
                            yield return t;
                            break;
                        case SerializableType serializableType:
                            yield return serializableType.Type;
                            break;
                    }
                }
            }
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            _rawGetter?.DrawError();
            GUILayout.BeginHorizontal();

            if (label != null)
                EditorGUILayout.PrefixLabel(label);

            var title = _title;
            if (string.IsNullOrWhiteSpace(title)) title = SmartValue?.Name;
            if (string.IsNullOrWhiteSpace(title)) title = "<None>";

            if (EditorGUILayout.DropdownButton(GUIContentHelper.TempContent(title), FocusType.Keyboard))
                DoTypeDropdown(_rect);
            
            GUILayout.EndHorizontal();
        }

        private void SetValue(Type type)
        {
            SmartValue = new SerializableType(type);
            GUI.changed = true;
        }

        private void DoTypeDropdown(Rect position)
        {
            if (_typePicker != null)
            {
                GenericPicker.Show(position, _typePicker);
                return;
            }
            
            IEnumerable<Type> list;
            if (_rawGetter != null)
                list = ResolveRawGetter();
            else if (_baseType != null)
                list = ReflectionUtility.GetTypesInheritingFrom(_baseType);
            else
            {
                if (_allTypes == null) // cache this between drawers so it doesn't get created multiple times
                    _allTypes = ReflectionUtility.GetTypesInheritingFrom(typeof(object));
                list = _allTypes;
            }
            
            _typePicker = GenericPicker.Show(position, SmartValue?.Type, list, SetValue);
        }
    }
}