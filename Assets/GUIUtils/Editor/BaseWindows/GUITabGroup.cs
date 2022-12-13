using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace GUIUtils.Editor.BaseWindows
{
    public class GUITabGroup
    {
        private GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false) };
        private GUITabPage currentPage;
        private GUITabPage targetPage;
        private Vector2 scrollPosition;
        private float currentHeight;
        private Dictionary<string, GUITabPage> pages = new Dictionary<string, GUITabPage>();

        /// <summary>The animation speed (1 / s)</summary>
        private float t = 1f;

        private bool isAnimating;
        private GUITabPage nextPage;
        private bool drawToolbar;
        private float toolbarHeight = 18f;
        private Rect toolbarRect;

        /// <summary>The animation speed</summary>
        public float AnimationSpeed = 4f;

        public bool FixedHeight;
        public bool ExpandHeight;

        private IEnumerable<GUITabPage> OrderedPages => this.pages
            .Select(x => x.Value)
            .OrderBy(x => x.Order);

        /// <summary>Gets the outer rect of the entire tab group.</summary>
        public Rect OuterRect { get; private set; }

        /// <summary>The inner rect of the current tab page.</summary>
        public Rect InnerRect { get; private set; }

        /// <summary>
        /// If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
        /// </summary>
        /// <summary>Sets the current page.</summary>
        /// <param name="page">The page to switch to.</param>
        public void SetCurrentPage(GUITabPage page)
        {
            this.currentPage = this.pages.ContainsValue(page)
                ? page
                : throw new InvalidOperationException("Page is not part of TabGroup");
            this.targetPage = null;
        }

        /// <summary>Gets the current page.</summary>
        public GUITabPage CurrentPage => this.targetPage ?? this.currentPage;

        /// <summary>Gets the t.</summary>
        public float T => this.t;

        internal bool IsAnimating => this.isAnimating;

        internal float InnerContainerWidth { get; private set; }

        internal float LabelWidth { get; private set; }

        /// <summary>The height of the tab buttons.</summary>
        public float ToolbarHeight
        {
            get => this.toolbarHeight;
            set => this.toolbarHeight = value;
        }

        /// <summary>Registers the tab.</summary>
        public GUITabPage RegisterTab(string title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            GUITabPage guiTabPage;
            if (!this.pages.TryGetValue(title, out guiTabPage))
                guiTabPage = this.pages[title] = new GUITabPage(this, title);
            return guiTabPage;
        }

        /// <summary>Begins the group.</summary>
        /// <param name="drawToolbar">if set to <c>true</c> a tool-bar for changing pages is drawn.</param>
        /// <param name="style">The style.</param>
        public void BeginGroup(bool drawToolbar = true, GUIStyle style = null)
        {
            this.LabelWidth = EditorGUIUtility.labelWidth;
            if (Event.current.type == EventType.Layout)
                this.drawToolbar = drawToolbar;
            style = style ?? CustomGUIStyles.ToggleGroupBackground;
            this.InnerContainerWidth = this.OuterRect.width -
                                       (style.padding.left + style.padding.right + style.margin.left +
                                        style.margin.right);
            if (this.currentPage == null && this.pages.Count > 0)
                this.currentPage = this.pages
                    .Select(x => x.Value)
                    .OrderBy(x => x.Order).First();
            if (this.currentPage != null && !this.pages.ContainsKey(this.currentPage.Name))
                this.currentPage = this.pages.Count <= 0 ? null : this.OrderedPages.First();
            float num1 = 0.0f;
            foreach (GUITabPage guiTabPage in this.pages.Values)
            {
                guiTabPage.OnBeginGroup();
                Rect rect = guiTabPage.Rect;
                num1 = Mathf.Max(rect.height, num1);
                if (Event.current.type == EventType.Layout && guiTabPage.IsVisible !=
                    (guiTabPage.IsVisible = guiTabPage == this.targetPage || guiTabPage == this.currentPage))
                {
                    if (this.targetPage == null)
                    {
                        this.scrollPosition.x = 0.0f;
                        rect = this.currentPage.Rect;
                        this.currentHeight = rect.height;
                    }
                    else
                    {
                        ref Vector2 local1 = ref this.scrollPosition;
                        double num2;
                        if (this.targetPage.Order < this.currentPage.Order)
                        {
                            ref Vector2 local2 = ref this.scrollPosition;
                            rect = this.OuterRect;
                            double width;
                            float num3 = (float) (width = rect.width);
                            local2.x = (float) width;
                            num2 = num3;
                        }
                        else
                            num2 = 0.0;

                        local1.x = (float) num2;
                        rect = currentPage.Rect;
                        currentHeight = rect.height;
                    }
                }
            }

            GUILayout.Space(1f);
            Rect rect1 = EditorGUILayout.BeginVertical(style,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(this.ExpandHeight));
            if (this.drawToolbar)
                this.DrawToolbar();
            if (this.InnerRect.width > 0.0 && !this.ExpandHeight)
            {
                if (this.options.Length == 2)
                {
                    if (this.currentPage != null)
                        this.currentHeight = this.currentPage.Rect.height;
                    this.options = new GUILayoutOption[] { 
                        GUILayout.ExpandWidth(true), 
                        GUILayout.ExpandHeight(this.ExpandHeight),
                        GUILayout.Height(this.currentHeight) 
                    };
                }

                this.options[2] = !this.FixedHeight ? GUILayout.Height(this.currentHeight) : GUILayout.Height(num1);
            }

            GUIContentHelper.PushDisabled(true);
            GUILayout.BeginScrollView(this.scrollPosition, false, false, GUIStyle.none, GUIStyle.none, this.options);
            GUIContentHelper.PopDisabled();
            Rect rect2 =
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(this.ExpandHeight));
            if (Event.current.type != EventType.Repaint)
                return;
            this.OuterRect = rect1;
            this.InnerRect = rect2;
        }

        /// <summary>Ends the group.</summary>
        public bool EndGroup()
        {
            EditorGUILayout.EndHorizontal();
            GUIContentHelper.PushDisabled(true);
            GUILayout.EndScrollView();
            GUIContentHelper.PopDisabled();
            EditorGUILayout.EndVertical();
            bool shouldRepaint = this.targetPage != this.currentPage;
            if (this.currentPage != null && Event.current.type == EventType.Repaint)
            {
                this.scrollPosition.x = this.currentPage.Rect.x;
                this.currentHeight = this.currentPage.Rect.height;
                if (this.targetPage != null && this.targetPage != this.currentPage && this.targetPage.IsVisible)
                {
                    this.currentPage.IsVisible = false;
                    this.currentPage = this.targetPage;
                    this.targetPage = (GUITabPage) null;
                    this.scrollPosition.x = 0.0f;
                    this.currentHeight = this.currentPage.Rect.height;
                    this.t = 1f;
                }
            }

            foreach (GUITabPage guiTabPage in this.pages.Values)
                guiTabPage.OnEndGroup();
            
            if (this.nextPage != null)
            {
                this.targetPage = this.nextPage;
                this.nextPage = (GUITabPage) null;
            }

            return shouldRepaint;
        }

        private void DrawToolbar()
        {
            if (Event.current.type == UnityEngine.EventType.Layout)
            {
                this.toolbarRect = this.OuterRect;
                this.toolbarRect.height = this.toolbarHeight;
                ++this.toolbarRect.x;
                --this.toolbarRect.width;
            }

            CustomEditorGUI.BeginHorizontalToolbar(this.toolbarHeight);
            foreach (GUITabPage orderedPage in this.OrderedPages)
            {
                if (orderedPage.IsActive &&
                    !GUILayout.Toggle(orderedPage == (this.nextPage ?? this.CurrentPage), orderedPage.Title, CustomGUIStyles.ToolbarTab, 
                        GUILayout.Height(22f)))
                    this.nextPage = orderedPage;
            }

            CustomEditorGUI.EndHorizontalToolbar();
            if (Event.current.type != EventType.Repaint)
                return;
            CustomEditorGUI.DrawBorders(new Rect(GUILayoutUtility.GetLastRect())
            {
                height = this.toolbarHeight
            }, 1, 1, 0, 0);
        }

        /// <summary>Goes to page.</summary>
        public void GoToPage(GUITabPage page) => this.nextPage = page;

        /// <summary>Goes to next page.</summary>
        public void GoToNextPage()
        {
            if (this.currentPage == null)
                return;
            bool flag = false;
            List<GUITabPage> list = this.OrderedPages.ToList();
            for (int index = 0; index < list.Count; ++index)
            {
                if (flag && list[index].IsActive)
                {
                    this.nextPage = list[index];
                    break;
                }

                if (list[index] == (this.nextPage ?? this.CurrentPage))
                    flag = true;
            }
        }

        /// <summary>Goes to previous page.</summary>
        public void GoToPreviousPage()
        {
            if (this.currentPage == null)
                return;
            List<GUITabPage> list = this.OrderedPages.ToList();
            int index1 = -1;
            for (int index2 = 0; index2 < list.Count; ++index2)
            {
                if (list[index2] == (this.nextPage ?? this.CurrentPage))
                {
                    if (index1 < 0)
                        break;
                    this.nextPage = list[index1];
                    break;
                }

                if (list[index2].IsActive)
                    index1 = index2;
            }
        }
    }
}