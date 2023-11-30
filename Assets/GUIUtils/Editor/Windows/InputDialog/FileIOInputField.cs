using System.IO;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class FileIOInputField : DialogInputField<string>
    {
        private readonly string _initialFolder;

        protected FileIOInputField(string label, string tooltip = null, string initialFolder = null, string initialValue = default(string)) 
            : base(label, tooltip, initialValue)
        {
            _initialFolder = initialFolder ?? Application.dataPath;
        }

        protected override void DrawFieldValue(Rect rect)
        {
            var iconRect = rect.AlignRight(16.0f);
            if (rect.IsValid())
                rect.width = rect.width - iconRect.width - CustomGUIUtility.Padding;
            
            SmartValue = EditorGUI.TextField(rect, SmartValue);
            if (CustomEditorGUI.IconButton(iconRect, UnityIcon.AssetIcon("Fa_Folder")))
            {
                var returnVal = OpenDialog(_initialFolder);
                if (!returnVal.IsNullOrEmpty())
                    SmartValue = returnVal;
            }
        }

        protected abstract string OpenDialog(string initialFolder);
    }

    public class SaveFileField : FileIOInputField
    {
        private string _defaultName;

        public SaveFileField(string label, string tooltip = null, string initialFolder = null, string initialValue = default(string)) 
            : base(label, tooltip, initialFolder, initialValue)
        {
            _defaultName = initialValue != null ? Path.GetFileNameWithoutExtension(initialValue) : "New File";
        }

        protected override string OpenDialog(string initialFolder)
        {
            return EditorUtility.SaveFilePanel("Save File...", initialFolder, _defaultName, "");
        }
    }
    
    public class OpenFolderField : FileIOInputField
    {
        public OpenFolderField(string label, string tooltip = null, string initialFolder = null, string initialValue = default(string)) 
            : base(label, tooltip, initialFolder, initialValue)
        {
        }

        protected override string OpenDialog(string initialFolder)
        {
            return EditorUtility.OpenFolderPanel("Open Folder...", initialFolder, string.Empty);
        }
    }
}