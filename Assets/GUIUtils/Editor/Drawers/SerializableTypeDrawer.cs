using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : BasePropertyDrawer<SerializableType, SerializableTypeDrawer.DrawerData>
    {
        public class DrawerData
        {
            public GenericHostInfo Info;
            
            public string TypeMethod;
            public string Title;
            public Type BaseType;
            public IPropertyMemberHelper RawGetter;
            public PickerHandler TypePicker;
            public bool IsDirty;
        }
        
        private static List<Type> _allTypesCache;

        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var data = new DrawerData
            {
                Info = info
            };
            
            if (info.TryGetAttribute(out AssignableTypeFilterAttribute assignableTypeFilter))
            {
                data.BaseType = assignableTypeFilter.BaseType;
                data.Title = assignableTypeFilter.DropdownTitle;
            }
            else if (info.TryGetAttribute(out TypeFilterAttribute typeFilter))
            {
                data.TypeMethod = typeFilter.MemberName;
                data.Title = typeFilter.DropdownTitle;

                data.RawGetter = MemberHelper.Create<object>(info, data.TypeMethod);
            }

            return data;
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data) => data.Info;

        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            var fullRect = position;
            data.RawGetter?.DrawError();
            
            if (label != null)
                position = EditorGUI.PrefixLabel(position, label);

            var title = data.Title;
            if (string.IsNullOrWhiteSpace(title)) title = SmartValue?.Name;
            if (string.IsNullOrWhiteSpace(title)) title = "<None>";

            if (EditorGUI.DropdownButton(position, GUIContentHelper.TempContent(title), FocusType.Keyboard))
                DoTypeDropdown(fullRect, data);

            if (data.IsDirty)
            {
                data.IsDirty = false;
                GUI.changed = true;
            }
        }

        private void SetValue(Type type, DrawerData data)
        {
            data.Info.SetValue(new SerializableType(type));
            data.IsDirty = true;
        }

        private void DoTypeDropdown(Rect position, DrawerData data)
        {
            if (data.TypePicker != null)
            {
                GenericPicker.Show(position, data.TypePicker);
                return;
            }
            
            ICollection<Type> list;
            if (data.RawGetter != null)
                list = ResolveRawGetter(data).ToArray();
            else if (data.BaseType != null)
                list = ReflectionUtility.GetTypesInheritingFrom(data.BaseType);
            else
            {
                if (_allTypesCache == null) // cache this between drawers so it doesn't get created multiple times
                    _allTypesCache = ReflectionUtility.GetTypesInheritingFrom(typeof(object));
                list = _allTypesCache;
            }
            
            data.TypePicker = GenericPicker.Show(position, SmartValue?.Type, list, x => SetValue(x, data));
        }
        
        private IEnumerable<Type> ResolveRawGetter(DrawerData data)
        {
            var result = data.RawGetter.GetValue();
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
    }
}