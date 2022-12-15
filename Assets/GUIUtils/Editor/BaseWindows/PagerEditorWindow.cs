using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public abstract class PagerEditorWindow<T> :
#if ODIN_INSPECTOR
        OdinEditorWindow where T : OdinEditorWindow
#else
        CustomEditorWindow where T : CustomEditorWindow
#endif
    {
        protected SlidePagedWindowNavigationHelper<object, T> _pager;

        private Vector2 _scrollPosition;
        protected bool _alwaysShowHorizontalScrollbar = false;
        protected bool _alwaysShowVerticalScrollbar = false;

        protected abstract object RootPage { get; }
        protected abstract string RootPageName { get; }

        public SlidePageNavigationHelper<object>.Page CurrentPage => _pager.GetCurrentPage();

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
            this.WindowPadding = Vector4.zero;

            if (this._pager != null) return;

            var windowType = this as T;
            _pager = new SlidePagedWindowNavigationHelper<object, T>(windowType);
            _pager.PushPage(RootPage, RootPageName);
            (RootPage as PagerPage<T>)?.SetPager(_pager);
        }

        protected virtual void Update()
        {
            if (CurrentPage?.Value is PagerPage<T> page)
                page.Update();
        }

        protected override void OnDestroy()
        {
            foreach (ITerminatable page in _pager.EnumeratePages.Select(x => x.Value).OfType<ITerminatable>())
                page.Terminate();

            base.OnDestroy();
        }

        protected virtual void Refresh()
        {
            var refreshable = _pager?.CurrentPage.Value as IRefreshable;
            refreshable?.Refresh();
        }

        protected virtual int DrawHeaderEditor()
        {
            return 0;
        }

        protected override void DrawEditors()
        {
            var contentRect = new Rect(0, 0, this.position.width, this.position.height);
            CustomEditorGUI.DrawSolidRect(contentRect, CustomGUIStyles.DarkEditorBackground);

            int headerHeight = DrawHeaderEditor();

            // Draw top pager:
            var headerRect = CustomEditorGUI.GetTopLevelLayoutRect().AlignTop(34).AddY(headerHeight);
            CustomEditorGUI.DrawSolidRect(headerRect, CustomGUIStyles.BoxBackgroundColor);
            CustomEditorGUI.DrawBorders(headerRect, 0, 0, 0, 1);
            var pageNavigationRect = headerRect;
            
            pageNavigationRect.y = (float) ((double) headerRect.y + ((double) headerRect.height * 0.5 - 20.0) * 0.5);
            pageNavigationRect.height = 20;
            pageNavigationRect = pageNavigationRect.HorizontalPadding(10);
            
            _pager.DrawPageNavigation(pageNavigationRect);
            // Draw pages:
            _pager.BeginGroup();
            
            var i = 0;
            foreach (var page in this._pager.EnumeratePages)
            {
                if (page.BeginPage())
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                    GUILayout.Space(30);
                    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, _alwaysShowHorizontalScrollbar, _alwaysShowVerticalScrollbar);
                    DrawEditor(i);
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck() && page.Value is PagerPage<T> odinPage)
                        odinPage.MarkAsChanged();

                }

                page.EndPage();
                i++;
            }

            _pager.EndGroup();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var currentPage = CurrentPage?.Value;
            (currentPage as IHasCustomMenu)?.AddItemsToMenu(menu);
        }

        protected override IEnumerable<object> GetTargets()
        {
            return _pager.EnumeratePages.Select(x => x.Value);
        }
    }
}