using System.IO;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute;

namespace Rhinox.GUIUtils.Editor
{
    public class FilePathWrapper : InlineButtonWrapper
    {
        private bool _chooseFolder;
        
        private bool _useBackslashes;
        private bool _absolute;
        private bool _requireExistingPath;
        
        private string _parentPath;
        private string _filters;

        public FilePathWrapper(IOrderedDrawable drawable) : base(drawable)
        {
        }

        protected override GUIContent GetContent()
        {
            return GUIContentHelper.TempContent(UnityIcon.AssetIcon("Fa_FolderOpen"));
        }

        protected override void Invoke()
        {
            string path = null;
            if (_chooseFolder)
                path = EditorUtility.OpenFolderPanel("Choose a folder", _parentPath, "");
            else 
                path = EditorUtility.OpenFilePanel("Choose a file", _parentPath, _filters);

            if (!string.IsNullOrEmpty(path))
            {
                if (ValidatePath(ref path))
                    SetValue(path);
            }
            
            GUIUtility.ExitGUI();
        }

        private bool ValidatePath(ref string path)
        {
            if (_absolute && !Path.IsPathRooted(path))
                path = Path.GetFullPath(path);

            if (_requireExistingPath)
            {
                if (_chooseFolder)
                {
                    if (!Directory.Exists(path))
                        return false;
                }
                else if (!File.Exists(path))
                    return false;
            }

            if (_useBackslashes)
                path = path.Replace("/", "\\");
            else
                path = path.Replace("\\", "/");

            return true;
        }

        [WrapDrawer(typeof(FolderPathAttribute), -5000)]
        public static BaseWrapperDrawable Create(FolderPathAttribute attr, IOrderedDrawable drawable)
        {
            return new FilePathWrapper(drawable)
            {
                _useBackslashes = attr.UseBackslashes,
                _absolute = attr.AbsolutePath,
                _parentPath = attr.ParentFolder,
                _requireExistingPath = attr.RequireExistingPath,
                _chooseFolder = true
            };
        }
        
        [WrapDrawer(typeof(FilePathAttribute), -5000)]
        public static BaseWrapperDrawable Create(FilePathAttribute attr, IOrderedDrawable drawable)
        {
            return new FilePathWrapper(drawable)
            {
                _useBackslashes = attr.UseBackslashes,
                _absolute = attr.AbsolutePath,
                _parentPath = attr.ParentFolder,
                _requireExistingPath = attr.RequireExistingPath,
                _filters = attr.Extensions
            };
        }
    }
}