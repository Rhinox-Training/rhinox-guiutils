using System;
using System.Collections.Generic;
using System.Linq;
using GUIUtils.Editor.BaseWindows;
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
        private List<SlidePageNavigationHelper<T>.Page> pages;
        private SlidePageNavigationHelper<T>.Page prev;
        public GUITabGroup TabGroup;

        public SlidePageNavigationHelper()
        {
            this.TabGroup = new GUITabGroup();
            this.TabGroup.AnimationSpeed = 4f;
            this.TabGroup.ExpandHeight = true;
            this.pages = new List<SlidePageNavigationHelper<T>.Page>();
        }

        public IEnumerable<SlidePageNavigationHelper<T>.Page> EnumeratePages
        {
            get
            {
                bool doPrev = true;
                for (int i = Math.Max(0, this.pages.Count - 3); i < this.pages.Count; ++i)
                {
                    SlidePageNavigationHelper<T>.Page page = this.pages[i];
                    if (page == this.prev)
                        doPrev = false;
                    yield return page;
                }

                if (this.prev != null & doPrev)
                    yield return this.prev;
            }
        }
        
        public SlidePageNavigationHelper<T>.Page GetCurrentPage()
        {
            if (pages == null)
                return null;
            Page page = this.pages.LastOrDefault();
            return page;
        }

        public void PushPage(T obj, string name)
        {
            GUITabPage tab = this.TabGroup.RegisterTab(Guid.NewGuid().ToString());
            SlidePageNavigationHelper<T>.Page page = new SlidePageNavigationHelper<T>.Page(obj, tab, name);
            this.pages.Add(page);
            this.TabGroup.GoToPage(page.Tab);
            this.prev = (SlidePageNavigationHelper<T>.Page) null;
        }

        public void NavigateBack()
        {
            if (this.IsOnFirstPage)
                return;
            this.prev = this.pages.Last<SlidePageNavigationHelper<T>.Page>();
            this.pages.RemoveAt(this.pages.Count - 1);
            this.TabGroup.GoToPage(this.pages[this.pages.Count - 1].Tab);
        }

        public void NavigateBack(int index)
        {
            if (this.IsOnFirstPage)
                return;
            this.prev = this.pages.Last<SlidePageNavigationHelper<T>.Page>();
            Resize(this.pages, index);
            this.TabGroup.GoToPage(this.pages[this.pages.Count - 1].Tab);
        }

        private static void Resize<T>(List<T> list, int length)
        {
            while (list.Count < length)
                list.Add(default (T));
            while (list.Count > length)
                list.RemoveAt(list.Count - 1);
        }

        public void DrawPageNavigation(Rect rect)
        {
            Rect rect1 = rect.AlignLeft(rect.height * 1.3f);
            bool oldGUIEnabled = GUI.enabled;
            GUI.enabled = !this.IsOnFirstPage;
            if (GUI.Button(rect1, GUIContent.none, GUIStyle.none))
                this.NavigateBack();

            var icon = UnityIcon.InternalIcon("GameObject Icon");
            var iconRect1 = rect1.AlignCenter(19f, 19f);
            GUI.DrawTexture(iconRect1, icon);
            
            GUI.enabled = oldGUIEnabled;
            rect.xMin += rect.height;
            int num = 0;
            for (int index = this.pages.Count - 1; index >= 0; --index)
            {
                SlidePageNavigationHelper<T>.Page page = this.pages[index];
                if (!page.TitleWidth.HasValue)
                    page.TitleWidth = new int?((int) CustomGUIStyles.Label.CalcSize(new GUIContent(page.Name)).x + 7);
                num += page.TitleWidth.Value;
            }

            rect.width -= 8f;
            float xMin = rect.xMin;
            if ((double) num > (double) rect.width)
                rect.xMin -= (float) num - rect.width;
            for (int index = 0; index < this.pages.Count; ++index)
            {
                SlidePageNavigationHelper<T>.Page page = this.pages[index];
                if (!page.TitleWidth.HasValue)
                    page.TitleWidth = new int?((int) CustomGUIStyles.Label.CalcSize(new GUIContent(page.Name)).x + 7);
                rect.width = (float) page.TitleWidth.Value;
                Rect rect2 = rect;
                rect2.width -= 6f;
                rect2.xMin = Mathf.Max(xMin, rect2.xMin);
                if (GUI.Button(rect2, page.Name, CustomGUIStyles.CenteredLabel))
                    this.NavigateBack(index + 1);
                if (index != this.pages.Count - 1)
                {
                    Rect position = rect2.AlignRight(10f);
                    position.x += 8f;
                    position.xMin = Mathf.Max(xMin, position.xMin);
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