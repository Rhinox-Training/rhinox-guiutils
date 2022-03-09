using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
    public static class SlidePageNavigationHelperExtensions
    {
        public static SlidePageNavigationHelper<T>.Page GetCurrentPage<T>(this SlidePageNavigationHelper<T> helper)
        {
            // Unfortunately EnumeratePages also includes the 'prev' page as last entry if it is not within the pages list (i.e. if you just navigated back)
            // Hence it cannot be used to determine what the current page is, you need to use the private field...

            // Another unfortunate: we cannot cache the FieldInfo as it differs for each type...
            // You can get the 'generic' fieldinfo BUT it cannot be used to retrieve a typed value
            var fieldInfo = typeof(SlidePageNavigationHelper<T>).GetField("pages", Flags.InstancePrivate);

            var list = (List<SlidePageNavigationHelper<T>.Page>) fieldInfo.GetValue(helper);
            return list.LastOrDefault();
        }
    }

    public class SlidePagedWindowNavigationHelper<T> : SlidePageNavigationHelper<T>
    {
        public OdinEditorWindow Window;

        public Page CurrentPage => this.GetCurrentPage();

        public SlidePagedWindowNavigationHelper(OdinEditorWindow window)
        {
            Window = window;
        }
    }

    public abstract class OdinPagerEditorWindow<T> : OdinEditorWindow where T : OdinEditorWindow
    {
        protected SlidePagedWindowNavigationHelper<object> _pager;

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

        protected virtual void Update()
        {
            if (CurrentPage?.Value is OdinPagerPage page)
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
            SirenixEditorGUI.DrawSolidRect(contentRect, SirenixGUIStyles.DarkEditorBackground);

            int headerHeight = DrawHeaderEditor();

            // Draw top pager:
            var headerRect = GUIHelper.GetCurrentLayoutRect().AlignTop(34).AddY(headerHeight);
            SirenixEditorGUI.DrawSolidRect(headerRect, SirenixGUIStyles.EditorWindowBackgroundColor);
            SirenixEditorGUI.DrawBorders(headerRect, 0, 0, 0, 1);
            _pager.DrawPageNavigation(headerRect.AlignCenterY(20).HorizontalPadding(10));

            // Draw pages:
            _pager.BeginGroup();
            var i = 0;
            foreach (var page in this._pager.EnumeratePages)
            {
                if (page.BeginPage())
                {
                    GUILayout.BeginVertical(GUILayoutOptions.ExpandHeight(true));
                    GUILayout.Space(30);
                    _scrollPosition =
                        GUILayout.BeginScrollView(_scrollPosition, _alwaysShowHorizontalScrollbar, _alwaysShowVerticalScrollbar);
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
            var currentPage = CurrentPage?.Value;
            (currentPage as IHasCustomMenu)?.AddItemsToMenu(menu);
        }

        protected override IEnumerable<object> GetTargets()
        {
            return _pager.EnumeratePages.Select(x => x.Value);
        }
    }
}