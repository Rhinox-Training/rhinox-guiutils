using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class PagerEditorWindow<T> :
#if ODIN_INSPECTOR
        OdinEditorWindow where T : OdinEditorWindow
#else
        CustomEditorWindow where T : CustomEditorWindow
#endif
    {
        protected SlidePageNavigationHelper<object> _pager;

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
#if ODIN_INSPECTOR
            this.WindowPadding = Vector4.zero;
#else
            this.WindowPadding = new RectOffset();
#endif

            if (this._pager != null) return;

            _pager = new SlidePageNavigationHelper<object>();
            _pager.UpdateRequestTarget(this);
            _pager.PushPage(RootPage, RootPageName);
        }
        
        protected virtual void Update()
        {
            if (CurrentPage?.Value is PagerPage page)
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
            var refreshable = CurrentPage.Value as IRefreshable;
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

            // Draw the pager
            var headerRect = contentRect.AlignTop(34).AddY(headerHeight);
            CustomEditorGUI.DrawSolidRect(headerRect, CustomGUIStyles.BoxBackgroundColor);
            CustomEditorGUI.DrawBorders(headerRect, 0, 0, 0, 1);
            
            var pageNavigationRect = headerRect
                .VerticalPadding((headerRect.height - 20) / 2)
                .HorizontalPadding(10)
                .SetHeight(20);
            
            _pager.DrawPageNavigation(pageNavigationRect);
            // Draw pages: (Note: BeginGroup also starts a scrollview)
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
                    DrawEditorOverlay(i);
                    if (EditorGUI.EndChangeCheck() && page.Value is PagerPage odinPage)
                        odinPage.MarkAsChanged();
                }

                page.EndPage();
                i++;
            }


            _pager.EndGroup();
        }

#if ODIN_INSPECTOR
        protected virtual void DrawEditorOverlay(int index)
        {
            if (CurrentPage.Value is PagerPage page)
                page.DrawTopOverlay();
        }
#else
        protected override void DrawEditorOverlay(int index)
        {
            base.DrawEditorOverlay(index);
            
            if (CurrentPage.Value is PagerPage page)
                page.DrawTopOverlay();
        }
#endif

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