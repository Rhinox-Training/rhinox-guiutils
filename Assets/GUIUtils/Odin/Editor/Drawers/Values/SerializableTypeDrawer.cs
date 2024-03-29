﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    [DrawerPriority(0, 0, 2003)]
    public class SerializableTypeDrawer : OdinValueDrawer<SerializableType>
    {
        // Yeah it's ugly but eh
        public class Null
        {
        }

        protected string _typeMethod;
        protected string _title;
        protected string _error;
        protected Type _baseType;

        protected InspectorPropertyValueGetter<object> _rawGetter;

        protected override void Initialize()
        {
            var typeFilter = Property.Attributes.OfType<TypeFilterAttribute>().FirstOrDefault();
            if (typeFilter != null)
            {
                _typeMethod = typeFilter.MemberName;
                _title = typeFilter.DropdownTitle;

                _rawGetter = new InspectorPropertyValueGetter<object>(this.Property, _typeMethod);
                _error = _rawGetter.ErrorMessage;
            }
        }

        private IEnumerable<Type> ResolveRawGetter()
        {
            var result = _rawGetter.GetValue();
            if (!(result is IEnumerable)) return Array.Empty<Type>();
            return ((IEnumerable)result).Cast<object>().Select(x =>
            {
                switch (x)
                {
                    case Type t:
                        return t;
                    case SerializableType serializableType:
                        return serializableType.Type;
                    default:
                        return null;
                }
            }).Where(x => x != null);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (_error != null)
            {
                SirenixEditorGUI.MessageBox(_error, MessageType.Error);
                return;
            }

            var rect = EditorGUILayout.GetControlRect();

            if (label != null)
                rect = EditorGUI.PrefixLabel(rect, label);

            var title = _title;
            if (string.IsNullOrWhiteSpace(title)) title = ValueEntry.SmartValue?.Name;
            if (string.IsNullOrWhiteSpace(title)) title = "<None>";

            EditorGUI.BeginChangeCheck();
            var types = TypeSelector.DrawSelectorDropdown(rect, GUIHelper.TempContent(title), CreateSelector);
            if (EditorGUI.EndChangeCheck())
            {
                var t = types?.FirstOrDefault();
                if (t == typeof(Null))
                    t = null;
                ValueEntry.SmartValue = new SerializableType(t);
            }
        }

        private TypeSelector CreateSelector(Rect position)
        {
            TypeSelector selector = null;
            if (_rawGetter != null)
                selector = new TypeSelector(ResolveRawGetter(), false);
            else if (_baseType != null)
                selector = new TypeSelector(ReflectionUtility.GetTypesInheritingFrom(_baseType), false);
            else
                selector = new TypeSelector(AssemblyTypeFlags.All, false);

            Type nullType = typeof(Null);
            selector.SelectionTree.MenuItems.Insert(0, new OdinMenuItem(selector.SelectionTree, "None", nullType));

            selector.SetSelection(ValueEntry.SmartValue);
            selector.EnableSingleClickToSelect();
            selector.FlattenTree = true;

            selector.ShowInPopup(position);
            return selector;
        }
    }
}