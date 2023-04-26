using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

public abstract class PickerHandler
{
    public object InitialValue;
    public IEnumerable Options;
    public readonly List<object> FilteredValues = new List<object>();

    public event Action OptionSelected;

    public static PickerHandler Create<T>(T initialValue, IEnumerable<T> options, Action<T> handleSelection)
        => new DefaultPickerHandler<T>(initialValue, options, handleSelection);

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
}

public class DefaultPickerHandler<T> : PickerHandler
{
    protected Action<T> _selectionHandler;

    public DefaultPickerHandler(T initialValue, IEnumerable<T> options, Action<T> handleSelection)
    {
        InitialValue = initialValue;
        Options = options;
        _selectionHandler = handleSelection;
    }

    public override void Select(object o)
    {
        _selectionHandler?.Invoke((T)o);
        base.Select(o);
    }

    protected override bool MatchesFilter(object value, string filter)
    {
        return string.IsNullOrEmpty(filter)
               || value.ToString().Contains(filter, StringComparison.InvariantCultureIgnoreCase);
    }
}

public class GenericPicker : PopupWindowContent
{
    private PageableReorderableList _listView;

    private PickerHandler _pickerHandler;
    private string _searchValue;
    private string _controlName = new GUID().ToString();

    private Vector2 _scrollPosition;
    private Vector2 _size;

    public event Action<object> OptionSelected;

    public int MaxOptionsShown
    {
        get => _listView.MaxItemsPerPage;
        set => _listView.MaxItemsPerPage = value;
    }

    public static PickerHandler Show<T>(Rect rect, T initialValue, IEnumerable<T> options, Action<T> handleSelection)
    {
        var picker = PickerHandler.Create(initialValue, options, handleSelection);
        Show(rect, picker);
        return picker;
    }

    private void OnDisable()
    {
        // _pickerHandler.OptionSelected -= Close;
    }

    public static void Show(Rect rect, PickerHandler handler)
    {
        var picker = new GenericPicker();
        picker.InitData(handler);


        var height = 20 + 20 + 4 + picker.MaxOptionsShown * 26;
        picker._size = new Vector2(rect.width, height);

        PopupWindow.Show(rect, picker);

        rect.height = height;

        // window.ShowAsDropDown(rect, new Vector2(rect.width, height));
    }

    public override Vector2 GetWindowSize() => _size;

    public void InitData(PickerHandler handler)
    {
        _pickerHandler = handler;
        // handler.OptionSelected += Close;

        _listView = new PageableReorderableList(_pickerHandler.FilteredValues, false, true, false, false);
        _listView.onSelectCallback += OnOptionSelected;
        _listView.RepaintRequested += OnRepaintRequested;

        MaxOptionsShown = 10;

        _pickerHandler.UpdateSearch(string.Empty);
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
        const int searchHeight = 20;
        var searchRect = rect.AlignTop(searchHeight);
        rect.y += searchHeight;

        GUI.SetNextControlName(_controlName);
        var newSearch = EditorGUI.TextField(searchRect, GUIContent.none, _searchValue, CustomGUIStyles.ToolbarSearchTextField);
        GUI.FocusControl(_controlName);

        if (newSearch != _searchValue)
        {
            _searchValue = newSearch;
            _pickerHandler.UpdateSearch(_searchValue);
        }

        _listView.DoList(rect, GUIContentHelper.TempContent("Options"));
    }

    protected void OnRepaintRequested()
    {
        editorWindow.Repaint();
    }
}