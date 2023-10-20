using System;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;


namespace Rhinox.GUIUtils.Editor
{
    public class SimpleTableView
    {
        private readonly GUILayoutOption[][] _columnOptions;
        public int ColumnCount { get; }


        private GUIStyle _cellStyle = null;

        private GUIStyle CellStyle
        {
            get
            {
                if (_cellStyle == null)
                {
                    _cellStyle = new GUIStyle("RL Background")
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Normal
                    };
                }
                return _cellStyle;
            }
        }

        private GUIStyle _defaultHeaderStyle = null;
        private Rect _tableRect;
        private readonly string[] _rowHeaders;
        private readonly GUIStyle _headerStyle;

        private GUIStyle DefaultHeaderStyle
        {
            get
            {
                if (_defaultHeaderStyle == null)
                {
                    _defaultHeaderStyle = new GUIStyle(CustomGUIStyles.BoldTitleCentered);
                }
                return _defaultHeaderStyle;
            }
        }

        public SimpleTableView(string[] rowHeaders, GUILayoutOption[][] columnOptions = null,
            GUIStyle headerStyle = null)
        {
            if (rowHeaders == null) throw new ArgumentNullException(nameof(rowHeaders));
            if (columnOptions != null && columnOptions.Length != rowHeaders.Length)
                throw new ArgumentException(nameof(columnOptions));
            ColumnCount = rowHeaders.Length;
            _columnOptions = columnOptions;
            _rowHeaders = rowHeaders;
            _headerStyle = headerStyle ?? DefaultHeaderStyle;
        }

        public void BeginDraw()
        {
            var topLevelRect = CustomEditorGUI.GetTopLevelLayoutRect();

            
            GUILayout.BeginVertical(CustomGUIStyles.Clean, GUILayout.MaxWidth(topLevelRect.width));

            GUILayout.BeginHorizontal(CustomGUIStyles.Clean); //"RL Header"
            {
                for (int i = 0; i < _rowHeaders.Length; ++i)
                {
                    var header = _rowHeaders[i];
                    if (_columnOptions != null)
                        EditorGUILayout.LabelField(header, CustomGUIStyles.Clean, _columnOptions[i]); //_headerStyle
                    else
                        EditorGUILayout.LabelField(header, CustomGUIStyles.Clean); //_headerStyle
                }
            }
            GUILayout.EndHorizontal();
        }

        public void DrawRow(params object[] entries)
        {
            if (entries.Length != ColumnCount)
                return;

            GUILayout.BeginHorizontal(CustomGUIStyles.Clean); //CellStyle
            {
                for (int i = 0; i < entries.Length; ++i)
                {
                    var columnOptions = _columnOptions != null ? _columnOptions[i] : Array.Empty<GUILayoutOption>();

                    if (!columnOptions.Any(x => x.IsWidth()) && _tableRect.IsValid())
                        columnOptions = Utility.JoinArrays(columnOptions,
                            new[]
                            {
                                GUILayout.MaxWidth(
                                    (_tableRect.width - ((_rowHeaders.Length + 50) * CustomGUIUtility.Padding)) /
                                    _rowHeaders.Length)
                            });


                    GUILayout.BeginVertical(CustomGUIStyles.Clean, columnOptions);
                    var entry = entries[i];
                    if (entry is Action<GUILayoutOption[]> entryDrawer)
                    {
                        entryDrawer.Invoke(columnOptions);
                    }
                    else
                    {
                        var content = GUIContentHelper.TempContent(entry != null ? entry.ToString() : "<NULL>");
                        EditorGUILayout.LabelField(content, CustomGUIStyles.CleanLabelField, columnOptions);
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
        }

        public void EndDraw()
        {
            GUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();

            if (rect.IsValid())
            {
                _tableRect = rect;
            }
        }
    }
}