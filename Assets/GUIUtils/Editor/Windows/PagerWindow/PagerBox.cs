using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class PagerBox : IRepaintRequestHandler, IRepaintable
    {
        private SlidePageNavigationHelper<object> _pager;
        
        private object[] _currentPaintedTargets = Array.Empty<object>();

        public SlidePageNavigationHelper<object>.Page CurrentPage => _pager.CurrentPage;
        
        private IEditor[] _editors = Array.Empty<IEditor>();
        
        public PagerBox(object root, string name)
        {
            _pager = new SlidePageNavigationHelper<object>();
            _pager.PushPage(root, name);
        }

        
        public void Draw(GUIStyle style = null)
        {
            if (Event.current.type == EventType.Layout)
                UpdateEditors();
            
            GUILayout.BeginVertical(style ?? CustomGUIStyles.Box);
            const float headerHeight = 34;
            var headerRect = new Rect(0, CustomGUIUtility.Padding, EditorGUIUtility.currentViewWidth, headerHeight);
            CustomEditorGUI.DrawSolidRect(headerRect, CustomGUIStyles.DarkEditorBackground);
            CustomEditorGUI.DrawBorders(headerRect, 0, 0, 0, 1);
            
            var pageNavigationRect = headerRect
                .VerticalPadding((headerRect.height - 20) / 2)
                .HorizontalPadding(10)
                .SetHeight(20);
            
            _pager.DrawPageNavigation(pageNavigationRect);
            
            GUILayout.Space(headerHeight);
            
            // Draw pages:
            _pager.BeginGroup();

            var i = 0;
            foreach (var page in this._pager.EnumeratePages)
            {
                if (page.BeginPage())
                {
                    EditorGUI.BeginChangeCheck();
                    DrawEditor(i);
                    if (EditorGUI.EndChangeCheck() && page.Value is PagerPage pageHandler)
                        pageHandler.MarkAsChanged();
                }

                page.EndPage();
                i++;
            }

            _pager.EndGroup();
            
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draws the editor for the this.CurrentDrawingTargets[index].
        /// </summary>
        protected virtual void DrawEditor(int index)
        {
            try
            {
                IEditor editor = _editors[index];
                if (editor != null && editor.CanDraw())
                {
                    if (editor is IRepaintRequestHandler handler)
                        handler.UpdateRequestTarget(this);
                    editor.Draw();
                }
            }
            catch (ExitGUIException)
            {
                // Rethrow, this is supported behaviour
                throw;
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox(e.ToString(), MessageType.Error);
                Debug.LogException(e);
            }
        }
        
        private void UpdateEditors()
        {
            _currentPaintedTargets = _currentPaintedTargets ?? Array.Empty<object>();
            _editors = _editors ?? Array.Empty<IEditor>();
            IList<object> targetList = GetTargets().ToArray();
            if (_currentPaintedTargets.Length != targetList.Count)
            {
                if (_editors.Length > targetList.Count)
                {
                    int num = _editors.Length - targetList.Count;
                    for (int i = 0; i < num; ++i)
                    {
                        IEditor editor = _editors[_editors.Length - i - 1];
                        editor?.Destroy();
                    }
                }

                Array.Resize(ref _currentPaintedTargets, targetList.Count);
                Array.Resize(ref _editors, targetList.Count);
                RequestRepaint();
            }

            for (int index = 0; index < targetList.Count; ++index)
            {
                object obj = targetList[index];
                object currentTarget = _currentPaintedTargets[index];
                if (obj != currentTarget) // Has target at index changed?
                {
                    RequestRepaint();
                    _currentPaintedTargets[index] = obj;
                    
                    // Refresh editor
                    if (_editors[index] != null)
                        _editors[index].Destroy();
                    
                    // Create new editor
                    _editors[index] = EditorCreator.CreateEditorForTarget(obj);
                }
            }
        }
        
        protected IEnumerable<object> GetTargets()
        {
            return _pager.EnumeratePages.Select(x => x.Value);
        }
        
        protected virtual void Update()
        {
            if (CurrentPage?.Value is PagerPage page)
                page.Update();
        }

        public void PushPage(object o, string name)
        {
            _pager.PushPage(o, name);
        }
        
        public void NavigateBack()
        {
            _pager.NavigateBack();
        }

        private IRepaintable _target;
        public void UpdateRequestTarget(IRepaintable target)
        {
            _target = target;
        }

        public void RequestRepaint()
        {
            if (_target != null)
                _target.RequestRepaint();
        }
    }
}