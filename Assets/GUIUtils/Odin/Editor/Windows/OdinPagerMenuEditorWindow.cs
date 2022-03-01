using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public abstract class OdinPagerMenuEditorWindow<T> : OdinMenuEditorWindow where T : OdinMenuEditorWindow
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
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(670, 700);

            window.Show();

            return true;
        }

        protected override void Initialize()
        {
            this.WindowPadding = Vector4.zero;

            if (this._pager != null) return;

            _pager = new SlidePagedWindowNavigationHelper<object>(this);
            _pager.PushPage(RootPage, RootPageName);
            (RootPage as OdinPagerPage)?.SetPager(_pager);
        }

        protected override void OnDestroy()
        {
            foreach (ITerminatable page in _pager.EnumeratePages.Select(x => x.Value).OfType<ITerminatable>())
                page.Terminate();

            base.OnDestroy();
        }

        protected override void OnBeginDrawEditors()
        {
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            // Draws a toolbar 
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);

            // Draw paging
            var headerRect = GUIHelper.GetCurrentLayoutRect();
            _pager.DrawPageNavigation(headerRect.AlignCenterY(20).HorizontalPadding(10));

            GUILayout.FlexibleSpace();

            DrawToolbarIcons(toolbarHeight);

            SirenixEditorGUI.EndHorizontalToolbar();
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
                    GUILayout.BeginVertical(GUILayoutOptions.ExpandHeight(true));
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

        /// <summary>
        /// Utility func which you can use to simplify the search algorithm of the MenuTree
        /// </summary>
        protected bool SimpleSearch(OdinMenuItem item)
        {
            return item.SearchString.Contains(MenuTree.Config.SearchTerm, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}