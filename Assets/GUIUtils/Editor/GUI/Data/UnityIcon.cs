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
using Object = UnityEngine.Object;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

namespace Rhinox.GUIUtils.Editor
{
    public class InternalUnityIcon : UnityIcon
    {
        public override string TextureUsage => $"UnityIcon.InternalIcon(\"{GetBestName()}\")";

        [HorizontalGroup("Variants"), ReadOnly, LabelText("Light/Dark (d_)"), ToggleLeft]
        public bool HasLightDarkVariants;

        [HorizontalGroup("Variants"), ReadOnly, LabelText("HighRes (@2x)"), ToggleLeft]
        public bool HasHighResVariants;

        private string GetBestName()
        {
            string name = Name;
            if (HasHighResVariants)
                name += "@2x";
            return name;
        }

        /// ================================================================================================================
        /// STATIC
        private static bool IsValidInternalTexture(Texture tex)
        {
            if (tex.name.Length == 0)
                return false;

            if (tex.hideFlags != HideFlags.HideAndDontSave &&
                tex.hideFlags != (HideFlags.HideInInspector | HideFlags.HideAndDontSave))
                return false;

            return EditorUtility.IsPersistent(tex);
        }

        public static Texture2D[] GetAll()
        {
            // FindObjectsOfTypeAll only does loaded; so load all
            return AssetDatabase.LoadAllAssetsAtPath("Library/unity editor resources")
                .OfType<Texture2D>()
                .Where(IsValidInternalTexture)
                .ToArray();
        }

        public static string TrimmedName(Texture2D tex)
        {
            string name = tex.name;
            if (name.StartsWith("d_"))
                name = name.Substring(2);
            if (name.EndsWith("@2x"))
                name = name.Substring(0, name.Length - 3);
            return name;
        }
    }

    public class AssetUnityIcon : UnityIcon
    {
        public string Extension { get; set; }

        public override string TextureUsage =>
            $"UnityIcon.AssetIcon(\"{Name}{(Extension == ".png" ? "" : $", \"{Extension}\"")}\")";

        /// ================================================================================================================
        /// STATIC
        public static readonly string[] IconFolders = new[]
        {
            "Assets/Editor/Icons",
            "Assets/Icons",

            "Assets/Plugins/Editor/Icons",

            "Assets/GUIUtils/Icons",
            "Packages/com.rhinox.open.guiutils/Icons"
        };

        public static Texture2D Find(string name, string ext)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(name);
            if (asset != null) return asset;

            foreach (var folder in AssetUnityIcon.IconFolders)
            {
                var path = FormatAssetsPath(folder);

                if (!AssetDatabase.IsValidFolder(path))
                    continue;

                var iconAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(path, name + ext));

                if (iconAsset)
                    return iconAsset;
            }

            return null;
        }

        public static string[] GetAll()
        {
            var folders = IconFolders
                .Select(FormatAssetsPath)
                .Where(AssetDatabase.IsValidFolder)
                .ToArray();

            if (!folders.Any()) return Array.Empty<string>();

            return AssetDatabase.FindAssets("t:Texture", searchInFolders: folders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
        }

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

    public class ScriptUnityIcon : UnityIcon
    {
        public override string TextureUsage => $"UnityIcon.AssetIcon(\"{Name}\")";

        /// ================================================================================================================
        /// STATIC
        public static Texture2D[] GetAll()
        {
            var allScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            return allScripts.Select(GetIconForObject).Where(x => x != null).ToArray();
        }

        private static MethodInfo _getIconForType;
        private static readonly object[] _arr = new object[1];

        private static Texture2D GetIconForObject(UnityEngine.Object o)
        {
            // TODO: made public in 2021.2
            if (_getIconForType == null)
            {
                // EditorGUIUtility.GetIconForObject
                var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.EditorGUIUtility");
                _getIconForType = type.GetMethod("GetIconForObject",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }

            _arr[0] = o;
            return (Texture2D) _getIconForType.Invoke(null, _arr);
        }
    }

    public class OdinUnityIcon : UnityIcon
    {
        public override string TextureUsage => "EditorIcons.{Name}.Active";

        /// ================================================================================================================
        /// STATIC
        public static Dictionary<string, Texture> GetAll()
        {
#if ODIN_INSPECTOR
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
#else
            return new Dictionary<string, Texture>();
#endif
        }

        public static Texture2D Find(string name)
        {
            var odinIcons = GetAll();
            if (odinIcons.ContainsKey(name))
                return odinIcons[name] as Texture2D;
            return null;
        }
    }

    /// <summary>
    /// A struct used for displaying all icons used in UnityIconsViewer
    /// Also used to find corresponding textures
    /// </summary>
    public abstract class UnityIcon : IEquatable<UnityIcon>, IComparable<UnityIcon>
    {
        /// ================================================================================================================
        /// EDITOR DISPLAY
        [ReadOnly, HideLabel, Title("Info")] [HorizontalGroup("H"), VerticalGroup("H/Left", 0)]
        public string Name;

        [ReadOnly] [VerticalGroup("H/Left", 0)]
        public string Origin;

        [HideLabel, PreviewField(70, ObjectFieldAlignment.Right)] [HorizontalGroup("H/Right", 70, 1)] [ReadOnly]
        public Texture Icon;

        public abstract string TextureUsage { get; }

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

        public static Texture2D ScriptIcon(string name)
        {
            // There is a proper method but it is internal so...
            var iconContent = EditorGUIUtility.IconContent(name);
            return iconContent?.image as Texture2D;
        }

        public static Texture2D AssetIcon(string name, string ext = ".png") => AssetUnityIcon.Find(name, ext);

        public static Texture2D OdinIcon(string name) => OdinUnityIcon.Find(name);
    }

    public static class UnityIconFinder
    {
        public static void FindIcons(ref List<UnityIcon> icons)
        {
            icons.Clear();

            // Internal icons
            var textures = InternalUnityIcon.GetAll();
            foreach (var group in textures.GroupBy(x => InternalUnityIcon.TrimmedName(x)))
            {
                if (group.Count() == 1)
                {
                    var tex = group.First();
                    icons.Add(new InternalUnityIcon
                    {
                        Icon = tex,
                        Name = tex.name,
                        Origin = "Internal",
                    });
                }
                else
                {
                    Texture2D darkTex = null, lightTex = null, highresDarkTex = null, highresLightTex = null;
                    foreach (var texture in group)
                    {
                        if (texture.name.EndsWith("@2x"))
                        {
                            if (texture.name.StartsWith("d_"))
                                highresDarkTex = texture;
                            else
                                highresLightTex = texture;
                        }
                        else
                        {
                            if (texture.name.StartsWith("d_"))
                                darkTex = texture;
                            else
                                lightTex = texture;
                        }
                    }

                    Texture2D tex;

                    if (EditorGUIUtility.isProSkin)
                        tex = highresDarkTex ?? darkTex ?? highresLightTex ?? lightTex;
                    else
                        tex = highresLightTex ?? lightTex ?? highresDarkTex ?? darkTex;

                    bool hasDarkLightVariants = darkTex && lightTex;
                    bool hasHighResVariants = highresDarkTex || highresLightTex;

                    icons.Add(new InternalUnityIcon
                    {
                        Icon = tex,
                        Name = hasDarkLightVariants ? lightTex.name : tex.name,
                        HasLightDarkVariants = hasDarkLightVariants,
                        HasHighResVariants = hasHighResVariants,
                        Origin = "Internal",
                    });
                }
            }

            // Class icons
            textures = ScriptUnityIcon.GetAll();
            foreach (var tex in textures)
            {
                icons.Add(new ScriptUnityIcon
                {
                    Icon = tex,
                    Name = tex.name,
                    Origin = "Script"
                });
            }

            // Odin icons
            var odinIcons = OdinUnityIcon.GetAll();
            foreach (var pair in odinIcons)
            {
                icons.Add(new OdinUnityIcon
                {
                    Icon = pair.Value,
                    Name = pair.Key,
                    Origin = "Odin",
                });
            }

            // resources icons
            icons.AddRange(GetAssetIcons(AssetUnityIcon.GetAll()));

            icons.Sort();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }


        private static List<UnityIcon> GetAssetIcons(string[] iconPaths)
        {
            var icons = new List<UnityIcon>();
            foreach (var path in iconPaths)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (tex == null) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);

                icons.Add(new AssetUnityIcon
                {
                    Name = name,
                    Extension = ext,
                    Icon = tex,
                    Origin = "Asset Texture",
                });
            }

            return icons;
        }
    }
}