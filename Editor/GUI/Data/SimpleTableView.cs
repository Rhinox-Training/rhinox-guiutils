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
            private float _currentRenderWidth;

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

            public SimpleTableView(string[] rowHeaders, GUILayoutOption[][] columnOptions = null, GUIStyle headerStyle = null)
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
                _tableRect = EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal("RL Header");
                {
                    for (int i = 0; i < _rowHeaders.Length; ++i)
                    {
                        var header = _rowHeaders[i];
                        if (_columnOptions != null)
                            EditorGUILayout.LabelField(header, _headerStyle, _columnOptions[i]);
                        else
                            EditorGUILayout.LabelField(header, _headerStyle);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            public void DrawRow(params object[] entries)
            {
                if (entries.Length != ColumnCount)
                    return;
                
                var horizontalRect = EditorGUILayout.BeginHorizontal(CellStyle);
                {
                    for (int i = 0; i < entries.Length; ++i)
                    {
                        var columnOptions = _columnOptions != null ? _columnOptions[i] : Array.Empty<GUILayoutOption>();
                        
                        if (!columnOptions.Any(x => x.IsWidth()))
                            columnOptions = Utility.JoinArrays(columnOptions,
                                new[] {GUILayout.Width(_currentRenderWidth / _rowHeaders.Length)});
                        
                        var entry = entries[i];
                        if (entry is Action<GUILayoutOption[]> entryDrawer)
                        {
                            entryDrawer.Invoke(columnOptions);
                        }
                        else
                        {
                            var content = GUIContentHelper.TempContent(entry != null ? entry.ToString() : "<NULL>");
                            EditorGUILayout.LabelField(content, CustomGUIStyles.CenteredLabel, columnOptions);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            public void EndDraw()
            {
                EditorGUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint)
                {
                    _currentRenderWidth = _tableRect.width;
                }
                
            }
        }
}