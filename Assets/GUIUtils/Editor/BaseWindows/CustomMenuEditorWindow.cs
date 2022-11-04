using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class CustomMenuEditorWindow : CustomEditorWindow
    {
        [NonSerialized] private bool isDirty;
        [SerializeField] [HideInInspector] private float menuWidth = 180f;
        [NonSerialized] private CustomMenuTree menuTree;
        [NonSerialized] private object trySelectObject;
        [SerializeField] [HideInInspector] private List<string> selectedItems = new List<string>();
        [SerializeField] [HideInInspector] private bool resizableMenuWidth = true;
       
        private static readonly EventInfo onProjectChangedEvent = typeof (EditorApplication).GetEvent("projectChanged");
        public static readonly bool HasOnProjectChanged = onProjectChangedEvent != null;
        
        public static event Action OnProjectChanged
        {
            add
            {
                if (onProjectChangedEvent == null)
                    throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
                onProjectChangedEvent.AddEventHandler((object) null, (Delegate) value);
            }
            remove
            {
                if (onProjectChangedEvent == null)
                    throw new NotImplementedException("EditorApplication.projectChanged is not implemented in this version of Unity.");
                onProjectChangedEvent.RemoveEventHandler((object) null, (Delegate) value);
            }
        }
        
        private void ProjectWindowChanged() => this.isDirty = true;

        /// <summary>
        /// Called when the window is destroyed. Remember to call base.OnDestroy();
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (HasOnProjectChanged)
            {
                OnProjectChanged -= new Action(this.ProjectWindowChanged);
            }
            else
            {
                EditorApplication.projectWindowChanged -= new EditorApplication.CallbackFunction(this.ProjectWindowChanged);
            }
        }

        /// <summary>Builds the menu tree.</summary>
        protected abstract CustomMenuTree BuildMenuTree();

        /// <summary>Gets or sets the width of the menu.</summary>
        public virtual float MenuWidth
        {
            get => this.menuWidth;
            set => this.menuWidth = value;
        }

        /// <summary>Gets the menu tree.</summary>
        public CustomMenuTree MenuTree => this.menuTree;
        
        /// <summary>Forces the menu tree rebuild.</summary>
        public void ForceMenuTreeRebuild()
        {
            this.menuTree = this.BuildMenuTree();
            if (this.selectedItems.Count == 0 && !this.menuTree.HasSelection)
            {
                var menuItem = this.menuTree.Enumerate()
                    .FirstOrDefault((Func<UIMenuItem, bool>) (x => x.RawValue != null));
                if (menuItem != null)
                {
                    // TODO: no nested support
                    // menuItem.GetParentMenuItemsRecursive(false)
                    //     .ForEach<UIMenuItem>((Action<UIMenuItem>) (x => x.Toggled = true));
                    menuItem.Select();
                }
            }
            else if (!this.menuTree.HasSelection && this.selectedItems.Count > 0)
            {
                foreach (var menuItem in this.menuTree.Enumerate())
                {
                    if (this.selectedItems.Contains(menuItem.FullPath))
                        menuItem.Select(true);
                }
            }

            this.menuTree.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            this.Repaint();
            CustomEditorGUI.RemoveFocusControl();
            this.selectedItems = this.menuTree.Selection
                .Select<UIMenuItem, string>((Func<UIMenuItem, string>) (x => x.FullPath)).ToList<string>();
            EditorUtility.SetDirty((UnityEngine.Object) this);
        }

        /// <summary>
        /// Tries to select the menu item with the specified object.
        /// </summary>
        public void TrySelectMenuItemWithObject(object obj) => this.trySelectObject = obj;

        /// <summary>Draws the menu tree selection.</summary>
        protected override IEnumerable<object> GetTargets()
        {
            if (this.menuTree != null)
            {
                for (int i = 0; i < this.menuTree.SelectionCount; ++i)
                {
                    var menuItem = this.menuTree.Selection[i];
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
                if (Event.current.type == UnityEngine.EventType.Layout)
                {
                    bool flag = this.menuTree == null;
                    if (this.menuTree == null || this.isDirty)
                    {
                        this.ForceMenuTreeRebuild();
                        if (flag)
                            CustomMenuTree.ActiveMenuTree = this.menuTree;
                        if (HasOnProjectChanged)
                        {
                            OnProjectChanged -= new Action(this.ProjectWindowChanged);
                            OnProjectChanged += new Action(this.ProjectWindowChanged);
                        }
                        else
                        {
                            EditorApplication.projectWindowChanged -=
                                new EditorApplication.CallbackFunction(this.ProjectWindowChanged);
                            EditorApplication.projectWindowChanged +=
                                new EditorApplication.CallbackFunction(this.ProjectWindowChanged);
                        }

                        this.isDirty = false;
                    }

                    if (this.trySelectObject != null && this.menuTree != null)
                    {
                        var menuItem = this.menuTree.Enumerate()
                            .FirstOrDefault((Func<UIMenuItem, bool>) (x => x.RawValue == this.trySelectObject));
                        if (menuItem != null)
                        {
                            this.menuTree.ClearSelection();
                            menuItem.Select();
                            this.trySelectObject = (object) null;
                        }
                    }
                }

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(this.MenuWidth), GUILayout.ExpandHeight(true));
                Rect currentLayoutRect = CustomEditorGUI.GetTopLevelLayoutRect();
                if (this.menuTree != null)
                    this.menuTree.HandleRefocus(currentLayoutRect);
                EditorGUI.DrawRect(currentLayoutRect, new Color(1f, 1f, 1f, 0.035f));
                Rect rect = currentLayoutRect;
                rect.xMin = currentLayoutRect.xMax - 4f;
                rect.xMax += 4f;
            
                this.DrawMenu();
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                EditorGUI.DrawRect(CustomEditorGUI.GetTopLevelLayoutRect(), CustomGUIStyles.DarkEditorBackground);
                base.OnGUI();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                EditorGUI.DrawRect(rect.AlignCenter(1f), CustomGUIStyles.BorderColor);
                if (this.menuTree != null)
                    this.menuTree.Update();
                RepaintIfRequested();
        }

        /// <summary>The method that draws the menu.</summary>
        protected virtual void DrawMenu()
        {
            if (this.menuTree == null)
                return;
            this.menuTree.Draw();
        }
    }
}