using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.NoOdin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.NoOdin.Editor
{
    [CustomPropertyDrawer(typeof(DrawAsReferenceAttribute))]
    public class DrawAsReferenceDrawer : BasePropertyDrawer<object, DrawAsReferenceDrawer.DrawerData>
    {
        public class DrawerData
        {
            public GenericHostInfo Info;
            public TypePicker Picker;
        }
        
        private SerializedProperty _property;

        private bool _expanded = true;

        protected override float GetPropertyHeight(GUIContent label, in DrawerData data)
        {
            var height = base.GetPropertyHeight(label, in data);
            if (_expanded)
            {
                height += GetInnerDrawerHeight(label);
                height += CustomGUIUtility.Padding;
            }

            return height;
        }

        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            Rect dropdownPosition = position.AlignTop(EditorGUIUtility.singleLineHeight);
            position.y += EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;
            
            if (EditorGUI.DropdownButton(dropdownPosition, label, FocusType.Passive))
                data.Picker?.Show(dropdownPosition);

            CallInnerDrawer(position, GUIContent.none);

            // EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }

        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var type = info.GetReturnType(false);
            var typeOptions = TypeCache.GetTypesDerivedFrom(type);// ReflectionUtility.GetTypesInheritingFrom(type);
            
            var picker = new TypePicker(typeOptions);
            picker.OptionSelected += SetManagedReference;

            return new DrawerData
            {
                Info = info,
                Picker = picker
            };
        }
        
        private void SetManagedReference(Type type)
        {
            var value = type.CreateInstance();
            HostInfo.SetValue(value);
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data) => data.Info;
    }
}