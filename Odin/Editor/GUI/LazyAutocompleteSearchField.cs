using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public class LazyAutocompleteSearchField
    {
        static class Styles
        {
            public const float resultHeight = 20f;
            public const float resultsBorderWidth = 2f;
            public const float resultsMargin = 15f;
            public const float resultsLabelOffset = 2f;

            public static readonly GUIStyle entryEven;
            public static readonly GUIStyle entryOdd;
            public static readonly GUIStyle labelStyle;
            public static readonly GUIStyle resultsBorderStyle;

            static Styles()
            {
                entryOdd = new GUIStyle("CN EntryBackOdd");
                entryEven = new GUIStyle("CN EntryBackEven");
                resultsBorderStyle = new GUIStyle("hostview");

                labelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    richText = true
                };
            }
        }

        public string SearchString;
        public readonly int MaxResults = 15;

        readonly List<string> _results = new List<string>();

        private int _selectedIndex = -1;

        private SearchField _searchField;

        private Vector2 _previousMousePosition;
        private bool _selectedIndexByMouse;

        private bool _showResults;

        // private OdinSelector<T> _selector;

        public delegate void StringHandler(string s);

        public event StringHandler InputChanged;
        public event StringHandler Confirmed;

        /*public LazyAutocompleteSearchField(OdinSelector<T> selector)
        {
            _selector = selector;
            TypeSelector typeSelector = new TypeSelector(AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                .Where(x => !x.IsAbstract && x.IsClass && x.InheritsFrom<UnityEngine.Object>())
                .Where(x => !x.Assembly.FullName.StartsWith("Sirenix"))
                .OrderBy(x => x.Assembly.GetAssemblyTypeFlag())
                .ThenBy(x => x.Namespace)
                .ThenByDescending(x => x.Name), false);
            
            _selector.SelectionChanged += (Action<IEnumerable<System.Type>>) (types =>
            {
                System.Type type = types.FirstOrDefault<System.Type>();
                if (type == null)
                    return;
                this.targetType = type;
                this.odinContext = TypeExtensions.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(this.targetType, true);
                this.CreateMenuTree(true);
            });
            typeSelector.SetSelection(this.targetType);
            typeSelector.ShowInPopup(300f);
        }*/

        public void AddResult(string result)
        {
            _results.Add(result);
        }

        public void ClearResults()
        {
            _results.Clear();
        }

        public void OnToolbarGUI()
        {
            Draw(asToolbar: true);
        }

        public void OnGUI()
        {
            Draw(asToolbar: false);
        }

        private void Draw(bool asToolbar)
        {
            var rect = GUILayoutUtility.GetRect(1, 1, 18, 18, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            DoSearchField(rect, asToolbar);
            GUILayout.EndHorizontal();
            rect.y += 18;
            DoResults(rect);
        }

        private void DoSearchField(Rect rect, bool asToolbar)
        {
            if (_searchField == null)
            {
                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += OnDownOrUpArrowKeyPressed;
            }

            var result = asToolbar
                ? _searchField.OnToolbarGUI(rect, SearchString)
                : _searchField.OnGUI(rect, SearchString);

            if (result != SearchString && InputChanged != null)
            {
                InputChanged(result);
                _selectedIndex = -1;
                _showResults = true;
            }

            SearchString = result;

            if (HasSearchBarFocused())
            {
                RepaintFocusedWindow();
            }
        }

        private void OnDownOrUpArrowKeyPressed()
        {
            var current = Event.current;

            if (current.keyCode == KeyCode.UpArrow)
            {
                current.Use();
                _selectedIndex--;
                _selectedIndexByMouse = false;
            }
            else
            {
                current.Use();
                _selectedIndex++;
                _selectedIndexByMouse = false;
            }

            if (_selectedIndex >= _results.Count) _selectedIndex = _results.Count - 1;
            else if (_selectedIndex < 0) _selectedIndex = -1;
        }

        private void DoResults(Rect rect)
        {
            if (_results.Count <= 0 || !_showResults) return;

            // OdinSelector<string>.DrawSelectorDropdown()

            var current = Event.current;
            rect.height = Styles.resultHeight * Mathf.Min(MaxResults, _results.Count);
            rect.x = Styles.resultsMargin;
            rect.width -= Styles.resultsMargin * 2;

            var elementRect = rect;

            rect.height += Styles.resultsBorderWidth;
            GUI.Label(rect, "", Styles.resultsBorderStyle);

            var mouseIsInResultsRect = rect.Contains(current.mousePosition);

            if (mouseIsInResultsRect)
            {
                RepaintFocusedWindow();
            }

            var movedMouseInRect = _previousMousePosition != current.mousePosition;

            elementRect.x += Styles.resultsBorderWidth;
            elementRect.width -= Styles.resultsBorderWidth * 2;
            elementRect.height = Styles.resultHeight;

            var didJustSelectIndex = false;

            for (var i = 0; i < _results.Count && i < MaxResults; i++)
            {
                if (current.type == EventType.Repaint)
                {
                    var style = i % 2 == 0 ? Styles.entryOdd : Styles.entryEven;

                    style.Draw(elementRect, false, false, i == _selectedIndex, false);

                    var labelRect = elementRect;
                    labelRect.x += Styles.resultsLabelOffset;
                    GUI.Label(labelRect, _results[i], Styles.labelStyle);
                }

                if (elementRect.Contains(current.mousePosition))
                {
                    if (movedMouseInRect)
                    {
                        _selectedIndex = i;
                        _selectedIndexByMouse = true;
                        didJustSelectIndex = true;
                    }

                    if (current.type == EventType.MouseDown)
                    {
                        OnConfirm(_results[i]);
                    }
                }

                elementRect.y += Styles.resultHeight;
            }

            if (current.type == EventType.Repaint && !didJustSelectIndex && !mouseIsInResultsRect && _selectedIndexByMouse)
            {
                _selectedIndex = -1;
            }

            if ((GUIUtility.hotControl != _searchField.searchFieldControlID && GUIUtility.hotControl > 0)
                || (current.rawType == EventType.MouseDown && !mouseIsInResultsRect))
            {
                _showResults = false;
            }

            if (current.type == EventType.KeyUp && current.keyCode == KeyCode.Return && _selectedIndex >= 0)
            {
                OnConfirm(_results[_selectedIndex]);
            }

            if (current.type == EventType.Repaint)
            {
                _previousMousePosition = current.mousePosition;
            }
        }

        private void OnConfirm(string result)
        {
            SearchString = result;

            InputChanged?.Invoke(result);
            Confirmed?.Invoke(result);

            RepaintFocusedWindow();
            GUIUtility.keyboardControl = 0; // To avoid Unity sometimes not updating the search field text
        }

        private bool HasSearchBarFocused()
        {
            return GUIUtility.keyboardControl == _searchField.searchFieldControlID;
        }

        private static void RepaintFocusedWindow()
        {
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }
    }
}