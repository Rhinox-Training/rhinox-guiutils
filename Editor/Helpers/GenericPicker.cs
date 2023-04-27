﻿using System;
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

    public static PickerHandler Create<T>(T initialValue, ICollection<T> options, Action<T> handleSelection)
        => new DefaultPickerHandler<T>(initialValue, options, handleSelection);

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
}

public class DefaultPickerHandler<T> : PickerHandler
{
    protected Action<T> _selectionHandler;

    public DefaultPickerHandler(T initialValue, ICollection<T> options, Action<T> handleSelection)
        : base(initialValue, options, options.Count)
    {
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
    
    public static PickerHandler Show<T>(Rect rect, T initialValue, ICollection<T> options, Action<T> handleSelection)
    {
        var picker = PickerHandler.Create(initialValue, options, handleSelection);
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
        
        _listView = new PageableReorderableList(_pickerHandler.FilteredValues, false, true, false, false);
        _listView.onSelectCallback += OnOptionSelected;
        _listView.RepaintRequested += OnRepaintRequested;

        MaxOptionsShown = DefaultItemsPerPage;
        ShowSearchField = MaxOptionsShown < _pickerHandler.AmountOfOptions;

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