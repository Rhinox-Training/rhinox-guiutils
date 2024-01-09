using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class SimplePicker<T> : BasePicker
    {
        public event Action<T> OptionSelected;

        public override Vector2 GetWindowSize() => _size;

        protected SimplePicker()
        {
        }

        public SimplePicker(ICollection<T> options, Func<T, string> textSelector = null, Func<T, string> subtextSelector = null)
        {
            var filteredCollection = FilteredCollection.Create(options, textSelector, subtextSelector);
            InitData(filteredCollection);
        }

        public SimplePicker(FilteredCollection collectionFilter)
        {
            InitData(collectionFilter);
        }

        protected override void OnOptionSelected(object option)
        {
            OptionSelected?.Invoke((T) option);
        }

        public override void OnGUI(Rect rect)
        {
            if (ShowSearchField)
            {
                float searchHeight = EditorGUIUtility.singleLineHeight;
                var searchRect = rect.AlignTop(searchHeight);
                rect.yMin += searchHeight;

                GUI.SetNextControlName(_controlName);
                var newSearch = EditorGUI.TextField(searchRect, GUIContent.none, _searchValue, CustomGUIStyles.ToolbarSearchTextField);
                GUI.FocusControl(_controlName);

                if (newSearch != _searchValue)
                {
                    _searchValue = newSearch;
                    FilteredCollection.UpdateSearch(_searchValue);
                }
            }

            base.OnGUI(rect); // Forward events to this rect
            _listView.DoList(rect, GUIContentHelper.TempContent("Options"));
        }
    }
}