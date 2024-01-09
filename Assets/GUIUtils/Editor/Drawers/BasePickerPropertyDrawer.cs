using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BasePickerPropertyDrawer<TField, TPicker> : BasePickerPropertyDrawer<TField>
    {
        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var data = new DrawerData
            {
                Info = info,
                Picker = null,
                ActiveContent = new GUIContent()
            };

            var value = info.GetSmartValue<TField>();
            if (value != null)
            {
                data.ActiveContent.text = GetNameForSelection(value);
            }
            return data;
        }

        protected override void OnOptionSelected(object selection, string selectionText, DrawerData data)
        {
            TPicker pickerVal = (TPicker) selection;
            var val = GetValueForSelection(pickerVal);
            base.OnOptionSelected(val, selectionText, data);
        }

        protected sealed override string GetNameForSelection(object selection)
        {
            if (selection == null)
                return NULL_STRING;
            TPicker pickerVal;
            if (selection is TField fieldVal)
                pickerVal = ReverseLookup(fieldVal);    
            else
                pickerVal = (TPicker) selection;
            if (pickerVal == null)
                return NULL_STRING;
            return this.GetNameForSelection(pickerVal);
        }

        protected abstract TPicker ReverseLookup(TField fieldVal);

        protected abstract string GetNameForSelection(TPicker pickerVal);

        protected abstract TField GetValueForSelection(TPicker pickerVal);
    }

    public abstract class BasePickerPropertyDrawer<T> : BasePropertyDrawer<T, BasePickerPropertyDrawer<T>.DrawerData>
    {
        protected const string NULL_STRING = "<None>";
        
        public class DrawerData
        {
            public GenericHostInfo Info;
            public BasePicker Picker;
            public GUIContent ActiveContent;
        }
        
        protected GUIContent _noneContent;

        protected override DrawerData CreateData(GenericHostInfo info)
        {
            var data = new DrawerData
            {
                Info = info,
                Picker = null,
                ActiveContent = new GUIContent()
            };

            var value = info.GetSmartValue<T>();
            if (value != null)
            {
                data.ActiveContent.text = GetNameForSelection(value);
            }
            return data;
        }

        protected override GenericHostInfo GetHostInfo(DrawerData data) => data.Info;
        protected override void OnInitialize()
        {
            base.OnInitialize();

            _noneContent = new GUIContent(NULL_STRING);
        }

        protected override void DrawProperty(Rect position, ref DrawerData data, GUIContent label)
        {
            var dropdownRect = EditorGUI.PrefixLabel(position, label);
            
            var content = SmartValue == null ? _noneContent : data.ActiveContent;
            
            if (EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard))
                DoPickerDropdown(position, data);
        }

        protected virtual void DoPickerDropdown(Rect position, DrawerData data)
        {
            if (data.Picker == null)
            {
                data.Picker = BuildPicker(data);
                data.Picker.OptionSelectedGeneric += x => OnOptionSelected(x, GetNameForSelection(x), data);
            }
            data.Picker.Show(position);
        }

        protected abstract BasePicker BuildPicker(DrawerData data);

        protected virtual void OnOptionSelected(object selection, string selectionText, DrawerData data)
        {
            data.Info.SetValue(selection);
            data.ActiveContent.text = selectionText;
        }

        protected abstract string GetNameForSelection(object selection);
    }
}