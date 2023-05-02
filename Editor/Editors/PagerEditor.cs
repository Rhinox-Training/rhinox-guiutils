using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class PagerEditor<T> : MultiTargetEditor<T>
        where T : class
    {
        protected SlidePageNavigationHelper<object> _pager;
        
        protected abstract object RootPage { get; }
        protected abstract string RootPageName { get; }
        
        public SlidePageNavigationHelper<object>.Page CurrentPage => _pager.GetCurrentPage();
        
        protected override void Initialize()
        {
            base.UseScrollView = false;
            
            base.Initialize();
            
            if (this._pager != null) return;

            _pager = new SlidePageNavigationHelper<object>();
            _pager.PushPage(RootPage, RootPageName);
        }
        
        protected virtual void OnDestroy()
        {
            foreach (ITerminatable page in _pager.EnumeratePages.Select(x => x.Value).OfType<ITerminatable>())
                page.Terminate();
        }


        protected virtual void Refresh()
        {
            var refreshable = CurrentPage.Value as IRefreshable;
            refreshable?.Refresh();
        }

        protected override void DrawEditors()
        {
            // Draw the pager
            const float headerHeight = 34;
            var headerRect = new Rect(0, CustomGUIUtility.Padding, EditorGUIUtility.currentViewWidth, headerHeight);
            CustomEditorGUI.DrawSolidRect(headerRect, CustomGUIStyles.DarkEditorBackground);
            CustomEditorGUI.DrawBorders(headerRect, 0, 0, 0, 1);
            
            var pageNavigationRect = headerRect
                .VerticalPadding((headerRect.height - 20) / 2)
                .HorizontalPadding(10)
                .SetHeight(20);
            
            _pager.DrawPageNavigation(pageNavigationRect);

            GUILayout.Space(headerHeight + CustomGUIUtility.Padding * 2);

            DrawScriptField();
            
            // Draw pages:
            _pager.BeginGroup();

            var i = 0;
            foreach (var page in this._pager.EnumeratePages)
            {
                if (page.BeginPage())
                {
                    EditorGUI.BeginChangeCheck();
                    DrawEditor(i);
                    DrawEditorOverlay(i);
                    if (EditorGUI.EndChangeCheck() && page.Value is PagerPage pageHandler)
                        pageHandler.MarkAsChanged();
                }

                page.EndPage();
                i++;
            }

            _pager.EndGroup();
        }
        
        protected override IEnumerable<object> GetTargets()
        {
            return _pager.EnumeratePages.Select(x => x.Value);
        }
    }
}