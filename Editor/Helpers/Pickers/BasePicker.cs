using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BasePicker : PopupWindowContent
    {
        public bool ShowSearchField { get; set; }

        public int MaxOptionsShown
        {
            get => _listView.MaxItemsPerPage;
            set
            {
                _listView.MaxItemsPerPage = value;
                _listView.DisplayHeader = value < FilteredCollection.AmountOfOptions;
            }
        }

        protected PageableReorderableList _listView;
        private const int DefaultItemsPerPage = 10;

        protected virtual float MinWidth => 210;

        protected FilteredCollection FilteredCollection;
        protected string _searchValue;
        protected string _controlName = GUID.Generate().ToString();

        protected Vector2 _size;

        public void Show(Rect rect)
        {
            _size = new Vector2(Mathf.Max(MinWidth, rect.width), GetHeight());
            PopupWindow.Show(rect, this);
        }

        protected void InitData(FilteredCollection handler)
        {
            FilteredCollection = handler;

            _listView = new PageableReorderableList(FilteredCollection.FilteredValues)
            {
                Draggable = false,
                DisplayAdd = false,
                DisplayRemove = false
            };
            _listView.onSelectCallback += OnOptionSelected;
            _listView.RepaintRequested += OnRepaintRequested;
            _listView.drawElementCallback = DrawElement;

            MaxOptionsShown = DefaultItemsPerPage;
            ShowSearchField = MaxOptionsShown < FilteredCollection.AmountOfOptions;

            FilteredCollection.UpdateSearch(string.Empty);
        }

        protected float GetHeight()
        {
            var height = ShowSearchField ? EditorGUIUtility.singleLineHeight : 0f;
            return height + _listView.GetHeight();
        }

        private void OnOptionSelected(BetterReorderableList list)
        {
            OnOptionSelected(list.SelectedItem);
            editorWindow.Close();
        }

        protected abstract void OnOptionSelected(object option);

        protected void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var obj = FilteredCollection.FilteredValues[index];

            if (obj == null)
            {
                GUI.Label(rect, GUIContentHelper.TempContent("<None>"));
                return;
            }

            var text = FilteredCollection.GetTextFor(obj);
            var subText = FilteredCollection.GetSubTextFor(obj);

            var style = CustomGUIStyles.MiniLabel;
            var width = style.CalcMaxWidth(subText);

            GUI.Label(rect.AlignLeft(rect.width - width), text);
            GUI.Label(rect.AlignRight(width), subText, style);
        }

        protected void OnRepaintRequested()
        {
            editorWindow.Repaint();
        }
    }
}