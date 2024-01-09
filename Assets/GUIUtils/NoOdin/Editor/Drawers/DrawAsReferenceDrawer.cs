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
            var height = EditorGUIUtility.singleLineHeight;
            if (_expanded && SmartValue != null)
            {
                height += GetInnerDrawerHeight(label);
                height += CustomGUIUtility.Padding;
            }

            return height;
        }

        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            Rect dropdownPosition = position.AlignTop(EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + CustomGUIUtility.Padding;

            var hasValue = SmartValue != null;
            
            Rect dropdownRect;
            if (hasValue)
                _expanded = eUtility.Foldout(dropdownPosition, _expanded, label, out dropdownRect);
            else
                eUtility.Header(dropdownPosition, label, out dropdownRect, CustomGUIStyles.Label);

            string typeTitle = null;
            if (hasValue) 
                typeTitle = SmartValue.GetType().Name;
            else 
                typeTitle = $"{BasePicker.NoneContentLabel} [{HostInfo.GetReturnType().Name}]";

            data.Picker.ShowDropdown(dropdownRect, GUIContentHelper.TempContent(typeTitle));

            if (_expanded && hasValue)
            {
                GUIContentHelper.PushIndentLevel();
                CallInnerDrawer(position, GUIContent.none);
                GUIContentHelper.PopIndentLevel();
            }

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
            // Ensure expanded once we select something
            if (value != null)
                _expanded = true;
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data) => data.Info;
    }
}