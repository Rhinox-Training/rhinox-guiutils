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
                SetValue(path);
            
            GUIUtility.ExitGUI();
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