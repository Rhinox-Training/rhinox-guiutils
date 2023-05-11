using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

public abstract class PickerHandler
{
    public readonly object InitialValue;
    public readonly IEnumerable Options;
    public readonly int AmountOfOptions;
    public readonly List<object> FilteredValues = new List<object>();

    public event Action OptionSelected;

    public static PickerHandler Create<T>(
            T initialValue,
            ICollection<T> options,
            Action<T> handleSelection,
            Func<T, string> textSelector,
            Func<T, string> subtextSelector)
        => new DefaultPickerHandler<T>(initialValue, options, handleSelection, textSelector, subtextSelector);

    protected PickerHandler(object initialValue, IEnumerable options, int optionsCount)
    {
        InitialValue = initialValue;
        Options = options;
        AmountOfOptions = optionsCount + 1; // + 1 for Null
    }

    public virtual void Select(object o)
    {
        OptionSelected?.Invoke();
    }

    public void UpdateSearch(string filter)
    {
        FilteredValues.Clear();
        FilteredValues.Add(null);
        foreach (var value in Options)
        {
            if (MatchesFilter(value, filter))
                FilteredValues.Add(value);
        }
    }

    protected abstract bool MatchesFilter(object o, string filter);
    public abstract string GetTextFor(object o);
    public abstract string GetSubTextFor(object o);
}

public class DefaultPickerHandler<T> : PickerHandler
{
    protected Action<T> _selectionHandler;
    protected Func<T, string> _textSelector;
    protected Func<T, string> _subTextSelector;

    public DefaultPickerHandler(T initialValue, ICollection<T> options, Action<T> handleSelection, Func<T, string> textSelector, Func<T, string> subTextSelector)
        : base(initialValue, options, options.Count)
    {
        _selectionHandler = handleSelection;
        _textSelector = textSelector;
        _subTextSelector = subTextSelector;
    }

    public override void Select(object o)
    {
        _selectionHandler?.Invoke((T)o);
        base.Select(o);
    }

    protected override bool MatchesFilter(object value, string filter)
    {
        if (string.IsNullOrEmpty(filter))
            return true;
        
        var text = GetTextFor(value);
        if (!string.IsNullOrEmpty(text) && text.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            return true;
        
        var subText = GetSubTextFor(value);
        if (!string.IsNullOrEmpty(subText) && subText.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            return true;

        return false;
    }

    public override string GetTextFor(object o)
    {
        if (_textSelector == null) return o.ToString();
        return _textSelector.Invoke((T) o);
    }

    public override string GetSubTextFor(object o)
    {
        if (_subTextSelector == null) return null;
        return _subTextSelector.Invoke((T)o);
    }
}

public class GenericPicker : PopupWindowContent
{
    private PageableReorderableList _listView;
    private const int DefaultItemsPerPage = 10;
    
    private PickerHandler _pickerHandler;
    private string _searchValue;
    private string _controlName = GUID.Generate().ToString();

    private Vector2 _scrollPosition;
    private Vector2 _size;
    
    public bool ShowSearchField { get; set; }

    public event Action<object> OptionSelected;

    public int MaxOptionsShown
    {
        get => _listView.MaxItemsPerPage;
        set
        {
            _listView.MaxItemsPerPage = value;
            _listView.DisplayHeader = value < _pickerHandler.AmountOfOptions;
        }
    }
    
    public static PickerHandler Show<T>(
        Rect rect,
        T initialValue,
        ICollection<T> options,
        Action<T> handleSelection,
        Func<T, string> textSelector = null,
        Func<T, string> subtextSelector = null)
    {
        var picker = PickerHandler.Create(initialValue, options, handleSelection, textSelector, subtextSelector);
        Show(rect, picker);
        return picker;
    }

    public static void Show(Rect rect, PickerHandler handler)
    {
        var picker = new GenericPicker();
        picker.InitData(handler);
        
        picker._size = new Vector2(rect.width, picker.GetHeight());

        PopupWindow.Show(rect, picker);
    }

    private float GetHeight()
    {
        var height = ShowSearchField ? EditorGUIUtility.singleLineHeight : 0f;
        return height + _listView.GetHeight();
    }

    public override Vector2 GetWindowSize() => _size;

    public void InitData(PickerHandler handler)
    {
        _pickerHandler = handler;
        
        _listView = new PageableReorderableList(_pickerHandler.FilteredValues)
        {
            Draggable = false,
            DisplayAdd = false,
            DisplayRemove = false
        };
        _listView.onSelectCallback += OnOptionSelected;
        _listView.RepaintRequested += OnRepaintRequested;
        _listView.drawElementCallback = DrawElement;

        MaxOptionsShown = DefaultItemsPerPage;
        ShowSearchField = MaxOptionsShown < _pickerHandler.AmountOfOptions;

        _pickerHandler.UpdateSearch(string.Empty);
    }

    private void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
    {
        var obj = _pickerHandler.FilteredValues[index];

        if (obj == null)
        {
            GUI.Label(rect, GUIContentHelper.TempContent("<None>"));
            return;
        }
        
        var text = _pickerHandler.GetTextFor(obj);
        var subText = _pickerHandler.GetSubTextFor(obj);
        
        var style = CustomGUIStyles.MiniLabel;
        var width = style.CalcMaxWidth(subText);
        
        GUI.Label(rect.AlignLeft(rect.width - width), text);
        GUI.Label(rect.AlignRight(width), subText, style);
    }

    private void OnOptionSelected(BetterReorderableList list)
    {
        var option = list.SelectedItem;
        _pickerHandler.Select(option);
        OptionSelected?.Invoke(option);
        editorWindow.Close();
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
                _pickerHandler.UpdateSearch(_searchValue);
            }
        }

        _listView.DoList(rect, GUIContentHelper.TempContent("Options"));
    }

    protected void OnRepaintRequested()
    {
        editorWindow.Repaint();
    }
}