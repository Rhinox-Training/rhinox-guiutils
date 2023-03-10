using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

// TODO: cleanup class
namespace Rhinox.GUIUtils.Editor
{
    /// <summary>
    /// Various BuildSettings interactions
    /// </summary>
    public static class BuildUtils
    {
        // time in seconds that we have to wait before we query again when IsReadOnly() is called.
        public static float minCheckWait = 3;

        private static float lastTimeChecked;
        private static bool cachedReadonlyVal = true;

        /// <summary>
        /// A small container for tracking scene data BuildSettings
        /// </summary>
        public struct BuildScene
        {
            public int buildIndex;
            public GUID assetGUID;
            public string assetPath;
            public EditorBuildSettingsScene scene;
        }

        /// <summary>
        /// Check if the build settings asset is readonly.
        /// Caches value and only queries state a max of every 'minCheckWait' seconds.
        /// </summary>
        public static bool IsReadOnly()
        {
            var curTime = Time.realtimeSinceStartup;
            var timeSinceLastCheck = curTime - lastTimeChecked;

            if (!(timeSinceLastCheck > minCheckWait)) return cachedReadonlyVal;

            lastTimeChecked = curTime;
            cachedReadonlyVal = QueryBuildSettingsStatus();

            return cachedReadonlyVal;
        }

        /// <summary>
        /// A blocking call to the Version Control system to see if the build settings asset is readonly.
        /// Use BuildSettingsIsReadOnly for version that caches the value for better responsivenes.
        /// </summary>
        private static bool QueryBuildSettingsStatus()
        {
            // If no version control provider, assume not readonly
            if (!Provider.enabled) return false;

            // If we cannot checkout, then assume we are not readonly
            if (!Provider.hasCheckoutSupport) return false;

            //// If offline (and are using a version control provider that requires checkout) we cannot edit.
            //if (UnityEditor.VersionControl.Provider.onlineState == UnityEditor.VersionControl.OnlineState.Offline)
            //    return true;

            // Try to get status for file
            var status = Provider.Status("ProjectSettings/EditorBuildSettings.asset", false);
            status.Wait();

            // If no status listed we can edit
            if (status.assetList == null || status.assetList.Count != 1) return true;

            // If is checked out, we can edit
            return !status.assetList[0].IsState(Asset.States.CheckedOutLocal);
        }

        /// <summary>
        /// For a given Scene Asset object reference, extract its build settings data, including buildIndex.
        /// </summary>
        public static BuildScene GetBuildScene(Object sceneObject)
        {
            var entry = new BuildScene
            {
                buildIndex = -1,
                assetGUID = new GUID(string.Empty)
            };

            if (sceneObject as SceneAsset == null) return entry;

            entry.assetPath = AssetDatabase.GetAssetPath(sceneObject);
            entry.assetGUID = new GUID(AssetDatabase.AssetPathToGUID(entry.assetPath));

            var scenes = EditorBuildSettings.scenes;
            for (var index = 0; index < scenes.Length; ++index)
            {
                if (!entry.assetGUID.Equals(scenes[index].guid)) continue;

                entry.scene = scenes[index];
                entry.buildIndex = index;
                return entry;
            }

            return entry;
        }

        /// <summary>
        /// Enable/Disable a given scene in the buildSettings
        /// </summary>
        public static void SetBuildSceneState(BuildScene buildScene, bool enabled)
        {
            var modified = false;
            var scenesToModify = EditorBuildSettings.scenes;
            foreach (var curScene in scenesToModify.Where(curScene => curScene.guid.Equals(buildScene.assetGUID)))
            {
                curScene.enabled = enabled;
                modified = true;
                break;
            }

            if (modified) EditorBuildSettings.scenes = scenesToModify;
        }

        /// <summary>
        /// Display Dialog to add a scene to build settings
        /// </summary>
        public static void AddBuildScene(BuildScene buildScene, bool force = false, bool enabled = true)
        {
            if (force == false)
            {
                var selection = EditorUtility.DisplayDialogComplex(
                    "Add Scene To Build",
                    "You are about to add scene at " + buildScene.assetPath + " To the Build Settings.",
                    "Add as Enabled", // option 0
                    "Add as Disabled", // option 1
                    "Cancel (do nothing)"); // option 2

                switch (selection)
                {
                    case 0: // enabled
                        enabled = true;
                        break;
                    case 1: // disabled
                        enabled = false;
                        break;
                    default:
                        //case 2: // cancel
                        return;
                }
            }

            var newScene = new EditorBuildSettingsScene(buildScene.assetGUID, enabled);
            var tempScenes = EditorBuildSettings.scenes.ToList();
            tempScenes.Add(newScene);
            EditorBuildSettings.scenes = tempScenes.ToArray();
        }

        /// <summary>
        /// Display Dialog to remove a scene from build settings (or just disable it)
        /// </summary>
        public static void RemoveBuildScene(BuildScene buildScene, bool force = false)
        {
            var onlyDisable = false;
            if (force == false)
            {
                var selection = -1;

                var title = "Remove Scene From Build";
                var details =
                    $"You are about to remove the following scene from build settings:\n    {buildScene.assetPath}\n    buildIndex: {buildScene.buildIndex}\n\nThis will modify build settings, but the scene asset will remain untouched.";
                var confirm = "Remove From Build";
                var alt = "Just Disable";
                var cancel = "Cancel (do nothing)";

                if (buildScene.scene.enabled)
                {
                    details += "\n\nIf you want, you can also just disable it instead.";
                    selection = EditorUtility.DisplayDialogComplex(title, details, confirm, alt, cancel);
                }
                else
                {
                    selection = EditorUtility.DisplayDialog(title, details, confirm, cancel) ? 0 : 2;
                }

                switch (selection)
                {
                    case 0: // remove
                        break;
                    case 1: // disable
                        onlyDisable = true;
                        break;
                    default:
                        //case 2: // cancel
                        return;
                }
            }

            // User chose to not remove, only disable the scene
            if (onlyDisable)
            {
                SetBuildSceneState(buildScene, false);
            }
            // User chose to fully remove the scene from build settings
            else
            {
                var tempScenes = EditorBuildSettings.scenes.ToList();
                tempScenes.RemoveAll(scene => scene.guid.Equals(buildScene.assetGUID));
                EditorBuildSettings.scenes = tempScenes.ToArray();
            }
        }

        /// <summary>
        /// Open the default Unity Build Settings window
        /// </summary>
        public static void OpenBuildSettings()
        {
            EditorWindow.GetWindow(typeof(BuildPlayerWindow));
        }
    }
}