/*
 *	Created by Philippe Groarke on 2016-08-28.
 *	Copyright (c) 2016 Tarfmagougou Games. All rights reserved.
 *
 *	Dedication : I dedicate this code to Gabriel, who makes kickass extensions. Now go out and use awesome icons!
 */
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// A struct used for displaying all icons used in UnityIconsViewer
    /// Also used to find corresponding textures
    /// </summary>
    public struct UnityIcon : IEquatable<UnityIcon>, IComparable<UnityIcon>
    {
        /// ================================================================================================================
        /// STATIC
        public static readonly string[] IconFolders = new []
        {
            // @"_Plugins\Editor\Icons",
            "Assets/Editor/Icons",
            "Assets/Icons",
            
            "Assets/Plugins/Editor/Icons",
            
            "Assets/GUIUtils/Icons",
            "Packages/com.rhinox.open.guiutils/Icons"
        };
        
        /// ================================================================================================================
        /// EDITOR DISPLAY
        [ReadOnly, HideLabel, Title("Info")]
        [HorizontalGroup("Row1"), VerticalGroup("Row1/Left", 0)]
        public string Name;

        [ReadOnly]
        [HorizontalGroup("Row1"), VerticalGroup("Row1/Left", 0)]
        public string Origin;
        
        [HideLabel, PreviewField(70, ObjectFieldAlignment.Right)]
        [HorizontalGroup("Row1", 70), VerticalGroup("Row1/Right", 1)]
        public Texture Icon;
        
        public string TextureUsage { get; set; }
        
        [PropertySpace(10)]
        [Button(ButtonSizes.Medium)]
        private void CopyName()
        {
            Copy(Name);
        }

        [Button(ButtonSizes.Medium)]
        private void CopyTextureUsage()
        {
            Copy(TextureUsage);
        }

        private static void Copy(string text)
        {
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log("'" + text + "' copied to clipboard.");
        }

        /// ================================================================================================================
        /// COMPARISON
        public override bool Equals(object o)
        {
            return o is UnityIcon && this.Equals((UnityIcon) o);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public bool Equals(UnityIcon o)
        {
            return this.Name == o.Name;
        }

        public int CompareTo(UnityIcon o)
        {
            return this.Name.CompareTo(o.Name);
        }

        /// ================================================================================================================
        /// EASE OF ACCESS
        public static Texture2D InternalIcon(string name)
        {
            // There is a proper method but it is internal so...
            var iconContent = EditorGUIUtility.IconContent(name);
            return iconContent?.image as Texture2D;
        }

        public static Texture2D AssetIcon(string name, string ext = ".png")
        {
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            if (asset != null) return asset;

            foreach (var folder in IconFolders)
            {
                var path = UnityIcon.FormatAssetsPath(folder);
                
                if (!AssetDatabase.IsValidFolder(path))
                    continue;
                
                var iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(path, name + ext));
                
                if (iconAsset)
                    return iconAsset;
            }

            return null;
        }
        
        /// ================================================================================================================
        /// ICON LISTS
        private static bool IsValidTexture(Texture tex)
        {
            if (tex.name.Length == 0)
                return false;

            if (tex.hideFlags != HideFlags.HideAndDontSave &&
                tex.hideFlags != (HideFlags.HideInInspector | HideFlags.HideAndDontSave))
                return false;

            return EditorUtility.IsPersistent(tex);
        }
        
        public static Texture[] GetAllInternalIcons()
        {
            return Resources.FindObjectsOfTypeAll<Texture2D>()
                .OfType<Texture>() // optional, mostly for the warning when you don't do this
                .Where(IsValidTexture)
                .ToArray();
        }
        
        public static string[] GetAllAssetIcons()
        {
            var folders = IconFolders
                .Select(UnityIcon.FormatAssetsPath)
                .Where(AssetDatabase.IsValidFolder)
                .ToArray();

            if (!folders.Any()) return Array.Empty<string>();
            
            return AssetDatabase.FindAssets("t:Texture", searchInFolders: folders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }
        
#if ODIN_INSPECTOR
        public static Dictionary<string, Texture> GetAllOdinIcons()
        {
            return typeof(EditorIcons)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.PropertyType == typeof(EditorIcon) || f.PropertyType == typeof(Texture2D))
                .ToDictionary(
                    x => x.Name,
                    x =>
                    {
                        var val = x.GetValue(null, null);
                        return val as Texture2D ?? ((EditorIcon) val)?.Active;
                    });
        }
#endif
        
        public static string FormatAssetsPath(string assetFolderPath)
        {
            assetFolderPath = (assetFolderPath ?? "")
                .Replace("\\", "/")
                .TrimEnd('/');
            // if (!assetFolderPath.ToLower().StartsWith("assets/"))
            //     assetFolderPath = "Assets/" + assetFolderPath;

            return assetFolderPath;
        }
    }

    /// ================================================================================================================
    /// UnityIconsViewer
    public class UnityIconsViewer : CustomMenuEditorWindow
    {
        private readonly List<UnityIcon> _Icons = new List<UnityIcon>();
        private Vector2 _scrollPos;
        private GUIContent _refreshButton;
        
        [MenuItem("Tools/Icons List")]
        public static void ShowWindow()
        {
            var w = GetWindow<UnityIconsViewer>();
            w.titleContent = TitleContent;
        }

        public static GUIContent TitleContent
        {
            get { return new GUIContent("Icon List", UnityIcon.AssetIcon("Fa_Search")); }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            FindIcons();
        }

        protected override void OnBeginDrawEditors()
        {
            var toolbarHeight = MenuTree.ToolbarHeight;

            // Draws a toolbar with the name of the currently selected menu item.
            CustomEditorGUI.BeginHorizontalToolbar(height: toolbarHeight);
            {
                GUILayout.Label(_Icons.Count + " Icons found");

                if (CustomEditorGUI.ToolbarButton(new GUIContent("Refresh", UnityIcon.AssetIcon("Fa_Redo"))))
                    FindIcons();
            }
            CustomEditorGUI.EndHorizontalToolbar();
        }

        /* Find all textures and filter them to narrow the search. */
        void FindIcons()
        {
            _Icons.Clear();

            // Internal icons
            var textures = UnityIcon.GetAllInternalIcons();
            foreach (var tex in textures)
            {
                // TODO: why do this? to get the resource is memory? but it has no path...
                Resources.Load<Texture>("");

                _Icons.Add(new UnityIcon
                {
                    Icon = tex,
                    Name = tex.name,
                    Origin = "Internal",
                    // TextureUseage = "EditorGUIUtility.IconContent(\"" + tex.name + "\").image"
                    TextureUsage = "UnityIcon.InternalIcon(\"" + tex.name + "\")"
                });
            }

            // Odin icons
#if ODIN_INSPECTOR
            var odinIcons = UnityIcon.GetAllOdinIcons();
            foreach (var pair in odinIcons)
            {
                _Icons.Add(new UnityIcon
                {
                    Icon = pair.Value,
                    Name = pair.Key,
                    Origin = "Odin",
                    TextureUsage = "EditorIcons." + pair.Key + ".Active"
                });
            }

            // resources icons
            _Icons.AddRange(GetAssetIcons( UnityIcon.GetAllAssetIcons() ));
#endif
                

            _Icons.Sort();
            Resources.UnloadUnusedAssets();
            GC.Collect();

            Repaint();
        }

        private static List<UnityIcon> GetAssetIcons(string[] iconPaths)
        {
            var icons = new List<UnityIcon>();
            foreach (var path in iconPaths)
            {
                var tex = (Texture) AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
                if (tex == null) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);
                
                icons.Add(new UnityIcon
                {
                    Name = name,
                    Icon = tex,
                    Origin = "Asset Texture",
                    // TextureUsage = "AssetDatabase.LoadAssetAtPath<Texture>(\"" + path + "\")"
                    TextureUsage = "UnityIcon.AssetIcon(\"" + name + (ext == ".png" ? "" : ", \"" + ext + "\"") + "\")"
                });
            }

            return icons;
        }

        protected override CustomMenuTree BuildMenuTree()
        {
            var tree = new CustomMenuTree(); // true: multisearch
#if ODIN_INSPECTOR
            if (tree.DefaultMenuStyle != null)
                tree.DefaultMenuStyle.IconSize = 16.00f;
            tree.DrawSearchToolbar = true;
#endif

            foreach (var icon in _Icons)
                tree.Add(icon.Origin + "/" + icon.Name, icon, icon.Icon);

            tree.SortMenuItemsByName();

            return tree;
        }

#if !ODIN_INSPECTOR
        protected override void DrawEditor(int index)
        {
            var target = (UnityIcon)GetTargets().ElementAt(index);

            var propertyDrawer = new DrawablePropertyView(target);
            propertyDrawer.DrawLayout();
        }
#endif
    }
}