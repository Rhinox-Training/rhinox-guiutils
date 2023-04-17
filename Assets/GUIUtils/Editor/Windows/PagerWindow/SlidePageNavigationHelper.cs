using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor.Helpers
{
    public class SlidePagedWindowNavigationHelper<T> : SlidePageNavigationHelper<T> 
    {
        public EditorWindow Window;

        public Page CurrentPage => this.GetCurrentPage();

        public SlidePagedWindowNavigationHelper(EditorWindow window)
        {
            Window = window;
        }
    }
    
    public class SlidePageNavigationHelper<T>
    {
        protected List<Page> pages;
        protected Page prev;
        public GUITabGroup TabGroup;
        
        protected HoverTexture _icon;
        protected List<HoverRect> _pageTitles;
        
        public Rect LastDrawnRect { get; protected set; }

        public SlidePageNavigationHelper()
        {
            this.TabGroup = new GUITabGroup();
            this.TabGroup.AnimationSpeed = 4f;
            this.TabGroup.ExpandHeight = true;
            this.pages = new List<Page>();

            _icon = new HoverTexture(UnityIcon.InternalIcon("d_scrollleft@2x"));
            _pageTitles = new List<HoverRect>();
        }

        public IEnumerable<Page> EnumeratePages
        {
            get
            {
                bool doPrev = true;
                for (int i = Math.Max(0, pages.Count - 3); i < pages.Count; ++i)
                {
                    Page page = pages[i];
                    if (page == prev)
                        doPrev = false;
                    yield return page;
                }

                if (prev != null & doPrev)
                    yield return prev;
            }
        }
        
        public Page GetCurrentPage()
        {
            if (pages == null)
                return null;
            Page page = pages.LastOrDefault();
            return page;
        }

        public void PushPage(T obj, string name)
        {
            GUITabPage tab = TabGroup.RegisterTab(Guid.NewGuid().ToString());
            Page page = new Page(obj, tab, name);
            pages.Add(page);
            TabGroup.GoToPage(page.Tab);
            prev = null;
        }

        public void NavigateBack()
        {
            if (IsOnFirstPage)
                return;
            prev = pages.Last();
            pages.RemoveAt(pages.Count - 1);
            TabGroup.GoToPage(pages[pages.Count - 1].Tab);
        }

        public void NavigateBack(int index)
        {
            if (IsOnFirstPage)
                return;
            prev = pages.Last();
            Resize(pages, index);
            TabGroup.GoToPage(pages[pages.Count - 1].Tab);
        }

#pragma warning disable CS0693
        private static void Resize<T>(List<T> list, int length)
        {
            while (list.Count < length)
                list.Add(default (T));
            while (list.Count > length)
                list.RemoveAt(list.Count - 1);
        }
#pragma warning restore CS0693

        public void DrawPageNavigation(Rect rect)
        {
            if (rect.IsValid())
                LastDrawnRect = rect;
            
            Rect titleSpacingRect = rect.AlignLeft(rect.height * 1.3f).AlignCenter(19f, 19f);
            bool oldGUIEnabled = GUI.enabled;
            GUI.enabled = !IsOnFirstPage;
            if (CustomEditorGUI.IconButton(titleSpacingRect, _icon))
                NavigateBack();

            // if (GUI.Button(titleSpacingRect, GUIContent.none, GUIStyle.none))
            //     NavigateBack();

            // _icon.Draw(titleSpacingRect.AlignCenter(19f, 19f));

            GUI.enabled = oldGUIEnabled;
            rect.xMin += rect.height;
            int totalWidthPages = 0;
            for (int i = pages.Count - 1; i >= 0; --i)
            {
                Page page = pages[i];
                if (!page.TitleWidth.HasValue)
                    page.TitleWidth = (int) CustomGUIStyles.Label.CalcSize(new GUIContent(page.Name)).x + 7;
                totalWidthPages += page.TitleWidth.Value;
            }

            rect.width -= 8f;
            float startPages = rect.xMin;
            if (totalWidthPages > rect.width)
                rect.xMin -= totalWidthPages - rect.width;
            
            // Fill the hoverRect array to match pages
            if (_pageTitles.Count < pages.Count)
            {
                for (int i = _pageTitles.Count; i < pages.Count; ++i)
                    _pageTitles.Add(new HoverRect { ClickColor = new Color(.45f, .6f, 1f)});
            }
            
            for (int i = 0; i < pages.Count; ++i)
            {
                Page page = pages[i];
                if (!page.TitleWidth.HasValue)
                    page.TitleWidth = (int) CustomGUIStyles.Label.CalcSize(new GUIContent(page.Name)).x + 7;
                rect.width = page.TitleWidth.Value;
                Rect pageTitleRect = rect;
                pageTitleRect.width -= 6f;
                pageTitleRect.xMin = Mathf.Max(startPages, pageTitleRect.xMin);

                // Pushes ClickColor when clicking
                _pageTitles[i].PushColor(pageTitleRect);
                
                if (GUI.Button(pageTitleRect, page.Name, CustomGUIStyles.CenteredLabelWithHover))
                    NavigateBack(i + 1);
                
                _pageTitles[i].PopColor();
                
                if (i != pages.Count - 1)
                {
                    Rect position = pageTitleRect.AlignRight(12f);
                    position.x += 8f;
                    position.xMin = Mathf.Max(startPages, position.xMin);
                    GUI.Label(position, "/", CustomGUIStyles.CenteredLabel);
                }

                rect.x += rect.width;
            }
        }

        public bool IsOnFirstPage => this.pages.Count <= 1;

        public void BeginGroup() => this.TabGroup.BeginGroup(false, GUIStyle.none);

        public void EndGroup()
        {
            this.TabGroup.EndGroup();
            if (Event.current.type == EventType.MouseDown && Event.current.button == 4)
                Event.current.Use();
        }

        public class Page
        {
            public T Value;
            public string Name;
            internal int? TitleWidth;
            internal GUITabPage Tab;

            public bool BeginPage() => this.Tab.BeginPage();

            public void EndPage() => this.Tab.EndPage();

            public Page(T @object, GUITabPage tab, string name)
            {
                this.Value = @object;
                this.Name = name;
                this.Tab = tab;
            }
        }
    }
}