using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static partial class eUtility
    {
        // TODO: move to lightspeed?
        public static void CreateAssetsDirectory(string directory)
        {
            var directories = directory.Split('\\', '/', Path.PathSeparator);
            var currentPath = string.Empty;
            foreach (var dir in directories)
            {
                currentPath = Path.Combine(currentPath, dir);
                if (!AssetDatabase.IsValidFolder(currentPath))
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(currentPath), Path.GetFileName(currentPath));
            }
        }
        
        /// <summary>
        /// Selects a folder in the project window and shows its content.
        /// Opens a new project window, if none is open yet.
        /// </summary>
        /// <param name="folderInstanceID">The instance of the folder asset to open.</param>
        public static void ShowFolderContents(int folderInstanceID)
        {
            System.Type projectBrowserType;
            var projectBrowsers = GetProjectBrowserInstances(out projectBrowserType);

            // This is the internal method, which performs the desired action.
            // Should only be called if the project window is in two column mode.
            MethodInfo showFolderContents = projectBrowserType.GetMethod(
                "ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var instance in projectBrowsers)
                ShowFolderContentsInternal(instance, showFolderContents, folderInstanceID);
        }

        private static Object[] GetProjectBrowserInstances(out System.Type projectBrowserType)
        {
            // Find the internal ProjectBrowser class in the editor assembly.
            System.Reflection.Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
            projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");

            // Find any open project browser windows.
            Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(projectBrowserType);

            if (projectBrowserInstances.Length > 0)
            {
                return projectBrowserInstances;
            }

            EditorWindow projectBrowser = OpenNewProjectBrowser(projectBrowserType);
            return new Object[] {projectBrowser};
        }
        
        public static void ShowFolderContentsContaining(Object asset)
        {
            var prevAssetPath = AssetDatabase.GetAssetPath(asset);
            var dir = Path.GetDirectoryName(prevAssetPath)?.Replace("\\", "/");

            ShowFolderContents(dir);
        }

        public static void ShowFolderContents(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            
            Object dirAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
            int id = dirAsset.GetInstanceID();

            ShowFolderContents(id);
        }

        public static void ShowFolderContentsInternal(Object projectBrowser, MethodInfo showFolderContents, int folderInstanceID)
        {
            // Sadly, there is no method to check for the view mode.
            // We can use the serialized object to find the private property.
            SerializedObject serializedObject = new SerializedObject(projectBrowser);
            bool inTwoColumnMode = serializedObject.FindProperty("m_ViewMode").enumValueIndex == 1;

            if (!inTwoColumnMode)
            {
                // If the browser is not in two column mode, we must set it to show the folder contents.
                MethodInfo setTwoColumns = projectBrowser.GetType().GetMethod(
                    "SetTwoColumns", BindingFlags.Instance | BindingFlags.NonPublic);
                setTwoColumns.Invoke(projectBrowser, null);
            }

            bool revealAndFrameInFolderTree = true;
            showFolderContents.Invoke(projectBrowser, new object[] {folderInstanceID, revealAndFrameInFolderTree});
        }

        private static EditorWindow OpenNewProjectBrowser(System.Type projectBrowserType)
        {
            EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType);
            projectBrowser.Show();

            // Unity does some special initialization logic, which we must call,
            // before we can use the ShowFolderContents method (else we get a NullReferenceException).
            MethodInfo init = projectBrowserType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
            init.Invoke(projectBrowser, null);

            return projectBrowser;
        }

        public static string GetShownFolder()
        {
            //typeof(UnityEditor.ProjectBrowser)
            System.Type projectBrowserType;
            var projectBrowser = GetProjectBrowserInstances(out projectBrowserType).FirstOrDefault();
            var method =
                projectBrowserType.GetMethod("GetActiveFolderPath", BindingFlags.Instance | BindingFlags.NonPublic);
            return (string) method.Invoke(projectBrowser, null);
        }
    }
}