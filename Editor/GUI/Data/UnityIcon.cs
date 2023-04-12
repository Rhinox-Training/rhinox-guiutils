using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Sirenix.Utilities.Editor;
#endif
using UnityEditor;
using UnityEngine;
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
        [VerticalGroup("Row1/Left", 0)]
        public string Origin;
        
        [HideLabel, PreviewField(70, ObjectFieldAlignment.Right)]
        [HorizontalGroup("Row1/Right", 70, 1)]
        [ReadOnly]
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
}