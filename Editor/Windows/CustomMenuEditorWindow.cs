using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class CustomMenuEditorWindow : CustomEditorWindow
    {
        [NonSerialized] private bool isDirty;
        [SerializeField] [HideInInspector] private float menuWidth = 180f;
        [NonSerialized] private CustomMenuTree menuTree;
        [NonSerialized] private object trySelectObject;
        [SerializeField] [HideInInspector] private List<string> selectedItems = new List<string>();
        [HideInInspector] private Vector2 _menuScrollPosition;

        private void ProjectWindowChanged() => isDirty = true;

        /// <summary>
        /// Called when the window is destroyed. Remember to call base.OnDestroy();
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EditorApplication.projectChanged -= ProjectWindowChanged;
        }

        /// <summary>Builds the menu tree.</summary>
        protected abstract CustomMenuTree BuildMenuTree();

        /// <summary>Gets or sets the width of the menu.</summary>
        public virtual float MenuWidth
        {
            get => menuWidth;
            set => menuWidth = value;
        }

        /// <summary>Gets the menu tree.</summary>
        public CustomMenuTree MenuTree => menuTree;

        /// <summary>Forces the menu tree rebuild.</summary>
        public void ForceMenuTreeRebuild()
        {
            menuTree = BuildMenuTree();
            if (selectedItems.Count == 0 && !menuTree.HasSelection)
            {
                var menuItem = menuTree.Enumerate()
                    .FirstOrDefault(x => x.RawValue != null);
                if (menuItem != null)
                {
                    menuTree.TryExpandAllParentItems(menuItem);
                    menuItem.Select();
                }
            }
            else if (!menuTree.HasSelection && selectedItems.Count > 0)
            {
                foreach (var menuItem in menuTree.Enumerate())
                {
                    if (selectedItems.Contains(menuItem.FullPath))
                        menuItem.Select(true);
                }
            }

            menuTree.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(SelectionChangedType items)
        {
            Repaint();
            CustomEditorGUI.RemoveFocusControl();
            selectedItems = menuTree.Selection.Select(x => x.FullPath).ToList();
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Tries to select the menu item with the specified object.
        /// </summary>
        public void TrySelectMenuItemWithObject(object obj) => trySelectObject = obj;

        /// <summary>Draws the menu tree selection.</summary>
        protected override IEnumerable<object> GetTargets()
        {
            if (menuTree != null)
            {
                for (int i = 0; i < menuTree.SelectionCount; ++i)
                {
                    var menuItem = menuTree.Selection[i];
                    if (menuItem != null)
                    {
                        object obj = menuItem.GetInstanceValue();
                        if (obj != null)
                            yield return obj;
                    }
                }
            }
        }

        /// <summary>Draws the Odin Editor Window.</summary>
        protected override void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                bool flag = menuTree == null;
                if (menuTree == null || isDirty)
                {
                    ForceMenuTreeRebuild();
                    if (flag)
                        CustomMenuTree.ActiveMenuTree = menuTree;

                    EditorApplication.projectChanged -= ProjectWindowChanged;
                    EditorApplication.projectChanged += ProjectWindowChanged;

                    isDirty = false;
                }

                if (trySelectObject != null && menuTree != null)
                {
                    var menuItem = menuTree.Enumerate()
                        .FirstOrDefault(x => x.RawValue == trySelectObject);
                    if (menuItem != null)
                    {
                        menuTree.ClearSelection();
                        menuItem.Select();
                        trySelectObject = null;
                    }
                }
            }

            Rect rect;
            GUILayout.BeginHorizontal();
            {
                _menuScrollPosition = GUILayout.BeginScrollView(_menuScrollPosition, GUILayout.Width(MenuWidth));
                GUILayout.BeginVertical();
                {
                    Rect currentLayoutRect = CustomEditorGUI.GetTopLevelLayoutRect();
                    if (menuTree != null)
                        menuTree.HandleRefocus(currentLayoutRect);
                    EditorGUI.DrawRect(currentLayoutRect, new Color(1f, 1f, 1f, 0.035f));
                    rect = currentLayoutRect;
                    rect.xMin = currentLayoutRect.xMax - 4f;
                    rect.xMax += 4f;

                    DrawMenu();

                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    EditorGUI.DrawRect(CustomEditorGUI.GetTopLevelLayoutRect(), CustomGUIStyles.DarkEditorBackground);
                    base.OnGUI();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            
            EditorGUI.DrawRect(rect.AlignCenter(1f), CustomGUIStyles.BorderColor);
            
            if (menuTree != null)
                menuTree.Update();
            RepaintIfRequested();
        }

        /// <summary>The method that draws the menu.</summary>
        protected virtual void DrawMenu()
        {
            if (menuTree == null)
                return;
            menuTree.Draw();
        }
    }
}