using System;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BasePicker : PopupWindowContent
    {
        public const string NoneContentLabel = "<None>";
        protected static readonly GUIContent NoneContent = new GUIContent(NoneContentLabel);
        
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
        
        public event Action<object> OptionSelectedGeneric;

        protected PageableReorderableList _listView;
        private const int DefaultItemsPerPage = 10;

        protected virtual float MinWidth => 210;

        protected FilteredCollection FilteredCollection;
        protected string _searchValue;
        protected string _controlName = GUID.Generate().ToString();

        protected Vector2 _size;
        private bool _supportsIcons;

        public void ShowDropdown(Rect rect, GUIContent label, FocusType type = FocusType.Keyboard)
        {
            if (EditorGUI.DropdownButton(rect, label, type))
                Show(rect);
        }

        public void Show(Rect rect)
        {
            _size = new Vector2(Mathf.Max(MinWidth, rect.width), GetHeight());
            PopupWindow.Show(rect, this);
        }

        protected void InitData(FilteredCollection handler)
        {
            FilteredCollection = handler;
            int count = 0;
            foreach (var option in handler.Options)
            {
                if (handler.TryGetIconFor(option, out _))
                    ++count;
            }
            _supportsIcons = count > 0;

            FilteredCollection.UpdateSearch(string.Empty);
            
            _listView = new PageableReorderableList(FilteredCollection.FilteredValues, true)
            {
                Draggable = false,
                DisplayAdd = false,
                DisplayRemove = false,
                Collapsible = false
            };
            _listView.onSelectCallback += OnOptionSelected;
            _listView.RepaintRequested += OnRepaintRequested;
            _listView.drawElementCallback = DrawElement;

            MaxOptionsShown = DefaultItemsPerPage;
            ShowSearchField = MaxOptionsShown < FilteredCollection.AmountOfOptions;
        }

        protected float GetHeight()
        {
            var height = ShowSearchField ? EditorGUIUtility.singleLineHeight : 0f;
            return height + _listView.GetHeight();
        }

        private void OnOptionSelected(BetterReorderableList list)
        {
            OnOptionSelected(list.SelectedItem);
            OptionSelectedGeneric?.Invoke(list.SelectedItem);
            editorWindow.Close();
        }

        protected abstract void OnOptionSelected(object option);

        protected void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var obj = FilteredCollection.FilteredValues[index];

            if (obj == null)
            {
                GUI.Label(rect, NoneContent);
                return;
            }

            var text = FilteredCollection.GetTextFor(obj);
            var subText = FilteredCollection.GetSubTextFor(obj);

            var style = CustomGUIStyles.MiniLabel;
            var width = style.CalcMaxWidth(subText);

            if (_supportsIcons)
            {
                FilteredCollection.TryGetIconFor(obj, out var icon);
                rect.SplitX(0.2f * rect.width, rect.width - width, out Rect left, out Rect middle, out Rect right);
                GUI.DrawTexture(left, icon, ScaleMode.ScaleToFit);
                GUI.Label(middle, GUIContentHelper.TempContent(text));
                GUI.Label(right, subText, style);
            }
            else
            {
                GUI.Label(rect.AlignLeft(rect.width - width), GUIContentHelper.TempContent(text));
                GUI.Label(rect.AlignRight(width), subText, style);
            }
        }

        public override void OnGUI(Rect rect)
        {
            // NOTE: this check allows inheriting classes to restrict the scroll wheel check region dynamically
            if (rect.Contains(Event.current.mousePosition))
            {
                // Has scrolled in the list view
                if (Event.current.isScrollWheel && _listView != null)
                {
                    if (Event.current.delta.y > float.Epsilon)
                    {
                        if (_listView.MovePage(1))
                            OnRepaintRequested();
                    }
                    else if (Event.current.delta.y < -float.Epsilon)
                    {
                        if (_listView.MovePage(-1))
                            OnRepaintRequested();
                    }
                }
            }
        }

        protected void OnRepaintRequested()
        {
            editorWindow.Repaint();
        }
    }
}