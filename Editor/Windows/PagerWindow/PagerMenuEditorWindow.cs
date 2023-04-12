using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class PagerMenuEditorWindow<T> : CustomMenuEditorWindow where T : CustomMenuEditorWindow
    {
        protected SlidePagedWindowNavigationHelper<object> _pager;

        private Vector2 _scrollPosition;

        protected abstract object RootPage { get; }
        protected abstract string RootPageName { get; }

        protected virtual bool IsMenuAvailable => true;


        /// <summary>
        /// Returns whether a window was created.
        /// </summary>
        protected static bool GetOrCreateWindow(out T window)
        {
            window = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            if (window != null)
            {
                window.Focus();
                return false;
            }

            window = GetWindow<T>();
            window.position = CustomEditorGUI.GetEditorWindowRect().AlignCenter(670, 700);

            window.Show();

            return true;
        }

        protected override void Initialize()
        {
            this.WindowPadding = new RectOffset();

            if (this._pager != null) return;

            _pager = new SlidePagedWindowNavigationHelper<object>(this as T);
            _pager.PushPage(RootPage, RootPageName);
            (RootPage as PagerPage)?.SetPager(_pager);
        }

        protected override void OnDestroy()
        {
            foreach (ITerminatable page in _pager.EnumeratePages.Select(x => x.Value).OfType<ITerminatable>())
                page.Terminate();

            base.OnDestroy();
        }

        protected override void OnBeginDrawEditors()
        {
            var toolbarHeight = MenuTree.ToolbarHeight;

            // Draws a toolbar 
            CustomEditorGUI.BeginHorizontalToolbar(height: toolbarHeight);

            // Draw paging
            var headerRect = CustomEditorGUI.GetTopLevelLayoutRect();
            _pager.DrawPageNavigation(headerRect.AlignCenterVertical(20).HorizontalPadding(10));

            GUILayout.FlexibleSpace();

            DrawToolbarIcons(toolbarHeight);

            CustomEditorGUI.EndHorizontalToolbar();
        }

        protected virtual void DrawToolbarIcons(int toolbarHeight)
        {
        }

        protected override void DrawEditors()
        {
            _pager.BeginGroup();
            var i = 0;
            foreach (var page in this._pager.EnumeratePages)
            {
                if (page.BeginPage())
                {
                    GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                    DrawEditor(i);
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                }

                page.EndPage();
                i++;
            }

            _pager.EndGroup();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var currentPage = _pager.CurrentPage?.Value;
            (currentPage as IHasCustomMenu)?.AddItemsToMenu(menu);
        }

        protected override void DrawMenu()
        {
            using (new eUtility.DisabledGroup(!IsMenuAvailable))
                base.DrawMenu();
        }

        protected override IEnumerable<object> GetTargets()
        {
            return _pager.EnumeratePages.Select(x => x.Value);
        }

#if ODIN_INSPECTOR
        /// <summary>
        /// Utility func which you can use to simplify the search algorithm of the MenuTree
        /// </summary>
        protected bool SimpleSearch(OdinMenuItem item)
        {
            return item.SearchString.Contains(MenuTree.Config.SearchTerm, StringComparison.InvariantCultureIgnoreCase);
        }
#endif
    }
}