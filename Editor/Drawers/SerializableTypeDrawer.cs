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
            public TypePicker TypePicker;
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
#if !ODIN_INSPECTOR_3
                data.TypeMethod = typeFilter.MemberName;
#else
                data.TypeMethod = typeFilter.FilterGetter;
#endif
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
            if (SmartValue != null) title = SmartValue.Name;
            if (title.IsNullOrEmpty()) title = BasePicker.NoneContentLabel;

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
            SerializableType newValue = null;
            if (type != null)
                newValue = new SerializableType(type);
            data.Info.SetValue(newValue);
            data.IsDirty = true;
        }

        private void DoTypeDropdown(Rect position, DrawerData data)
        {
            if (data.TypePicker == null)
            {
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
                
                data.TypePicker = new TypePicker(list);
                data.TypePicker.OptionSelected += x => SetValue(x, data);
            }
            
            data.TypePicker.Show(position);
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