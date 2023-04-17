using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public abstract class SimpleOdinValueDrawer<T> : OdinValueDrawer<T>
    {
        private bool _fullInitialized = false;

        protected virtual void OnInitialized()
        {
            
        }
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (!_fullInitialized)
            {
                OnInitialized();
                _fullInitialized = true;
            }
            
            var propertyValueEntry = Property.ValueEntry as IPropertyValueEntry<T>;

            if (propertyValueEntry == null)
            {
                Property.Draw(label);
                return;
            }
            
            OnCustomDrawPropertyLayout(label, propertyValueEntry);
        }

        protected abstract void OnCustomDrawPropertyLayout(GUIContent label, IPropertyValueEntry<T> valueEntry);
        
        protected InspectorProperty GetChildProperty(string name, InspectorProperty parent = null)
        {
            if (parent == null)
                parent = Property;
            return parent.FindChild(x => x.Name.Equals(name, StringComparison.InvariantCulture), false);
        }

        protected InspectorProperty GetChildProperty<TChildType>(string name, out IPropertyValueEntry<TChildType> childValueEntry)
        {
            return GetChildProperty(name, Property, out childValueEntry);
        }
        
        protected InspectorProperty GetChildProperty<TChildType>(string name, InspectorProperty parent, out IPropertyValueEntry<TChildType> childValueEntry)
        {
            if (parent == null)
                parent = Property;
            var childProperty = parent.FindChild(x => x.Name.Equals(name, StringComparison.InvariantCulture), false);
            if (childProperty != null)
                childValueEntry = childProperty.ValueEntry as IPropertyValueEntry<TChildType>;
            else
                childValueEntry = null;
            return childProperty;
        }

        protected void DrawDropdown<TValue>(Rect rect, string name, ICollection<TValue> options, Action<TValue> selectionConfirmed, Func<TValue, string> getMenuItemName = null)
        {
            var selector = new GenericSelector<TValue>(name, false, getMenuItemName, options)
            {
                FlattenedTree = true
            };
                
            selector.EnableSingleClickToSelect();
            selector.SelectionConfirmed += x =>
            {
                var t = x.FirstOrDefault();
                
                if (t != null)
                    selectionConfirmed?.Invoke(t);
            };
                
                
            selector.ShowInPopup(rect);
        }

        protected void DrawTypeDropdown(Rect rect, ICollection<Type> types, Action<Type> selectionConfirmed)
        {
            var selector = new TypeSelector(types, false)
            {
                FlattenTree = true
            };
            selector.EnableSingleClickToSelect();
            selector.SelectionConfirmed += x =>
            {
                var t = x.FirstOrDefault();
                if (t != null)
                    selectionConfirmed?.Invoke(t);
            };
            selector.ShowInPopup(rect);
        }

        protected static void GetLabelFieldRects(out Rect labelRect, out Rect fieldRect)
        {
            GetLabelFieldRects(EditorGUILayout.GetControlRect(), out labelRect, out fieldRect);
        }

        protected static void GetLabelFieldRects(Rect rect, out Rect labelRect, out Rect fieldRect)
        {
            var labelWidth = GUIHelper.BetterLabelWidth;
            labelRect = rect.AlignLeft(labelWidth);
            fieldRect = rect.AlignRight(rect.width - labelWidth);
        }
        
        protected static void SplitRect(Rect rect, float middlePerc, out Rect leftRect, out Rect rightRect)
        {
            var labelWidth = GUIHelper.BetterLabelWidth;
            leftRect = rect.AlignLeft(rect.width * middlePerc);
            rightRect = rect.AlignRight(rect.width * (1.0f - middlePerc));
        }
    }
}