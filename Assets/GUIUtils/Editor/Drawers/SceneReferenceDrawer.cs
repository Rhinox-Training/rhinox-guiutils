using System;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    [CustomPropertyDrawer(typeof(SceneReferenceData), true)]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        private static readonly GUIStyle boxStyle = EditorStyles.helpBox;
        private static readonly RectOffset boxPadding = boxStyle.padding;

        private const float PAD_SIZE = 2f;
        private const float FOOTER_HEIGHT = 10f;

        private static readonly float lineHeight = EditorGUIUtility.singleLineHeight;
        private static readonly float paddedLine = lineHeight + PAD_SIZE;
        
        //protected override void DrawPropertyLayout(GUIContent label)
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.BeginVertical(boxStyle);
            using (new eUtility.IndentedLayout())
            {
                GUILayout.BeginHorizontal();

                if (label != null)
                    EditorGUILayout.PrefixLabel(label);

                // Draw scene selector
                SceneReferenceData smartValue = property.GetValue() as SceneReferenceData;
                var asset = smartValue.SceneAsset;

                var newAsset = EditorGUILayout.ObjectField(asset, typeof(SceneAsset), false);

                if (newAsset != asset)
                {
                    if (newAsset != null)
                    {
                        // Call Constructor taking a SceneAsset
                        smartValue = Activator.CreateInstance(fieldInfo.FieldType, new[] {newAsset}) as SceneReferenceData;
                        property.SetValue(smartValue);
                        property.serializedObject.ApplyModifiedProperties();
                        asset = newAsset;
                    }
                    else
                        smartValue.ScenePath = null;
                }

                // End of scene selector
                GUILayout.EndHorizontal();

                // Draw scene info
                GUILayout.BeginHorizontal();

                // Draw the Build Settings Info of the selected Scene
                var buildScene = BuildUtils.GetBuildScene(asset);

                if (!buildScene.assetGUID.Empty())
                    DrawSceneInfoGUI(buildScene);

                GUILayout.EndHorizontal();

            }
            EditorGUILayout.EndVertical();
        }

        private void DrawSceneInfoGUI(BuildUtils.BuildScene buildScene)
        {
            var readOnly = BuildUtils.IsReadOnly();
            var readOnlyWarning =
                readOnly ? "\n\nWARNING: Build Settings is not checked out and so cannot be modified." : "";

            // Label Prefix
            var iconContent = new GUIContent();
            var labelContent = new GUIContent();

            // Missing from build scenes
            if (buildScene.buildIndex == -1)
            {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close");
                labelContent.text = "NOT In Build";
                labelContent.tooltip = "This scene is NOT in build settings.\nIt will be NOT included in builds.";
            }
            // In build scenes and enabled
            else if (buildScene.scene.enabled)
            {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_max");
                labelContent.text = "BuildIndex: " + buildScene.buildIndex;
                labelContent.tooltip = "This scene is in build settings and ENABLED.\nIt will be included in builds." +
                                       readOnlyWarning;
            }
            // In build scenes and disabled
            else
            {
                iconContent = EditorGUIUtility.IconContent("d_winbtn_mac_min");
                labelContent.text = "BuildIndex: " + buildScene.buildIndex;
                labelContent.tooltip =
                    "This scene is in build settings and DISABLED.\nIt will be NOT included in builds.";
            }

            // Left status label
            Rect buttonRect;
            using (new EditorGUI.DisabledScope(readOnly))
            {
                var rect = EditorGUILayout.GetControlRect();
                var labelRect = RectExtensions.AlignLeft(rect, EditorGUIUtility.labelWidth);
                buttonRect = RectExtensions.AlignRight(rect, rect.width - EditorGUIUtility.labelWidth);

                var iconRect = labelRect;
                iconRect.width = iconContent.image.width + PAD_SIZE;
                labelRect.width -= iconRect.width;
                labelRect.x += iconRect.width;
                EditorGUI.LabelField(iconRect, iconContent);
                EditorGUI.LabelField(labelRect, labelContent);
            }

            // Right context buttons
            buttonRect.width /= 3;

            var tooltipMsg = "";
            using (new EditorGUI.DisabledScope(readOnly))
            {
                // NOT in build settings
                if (buildScene.buildIndex == -1)
                {
                    buttonRect.width *= 2;
                    var addIndex = EditorBuildSettings.scenes.Length;
                    tooltipMsg =
                        "Add this scene to build settings. It will be appended to the end of the build scenes as buildIndex: " +
                        addIndex + "." + readOnlyWarning;
                    if (EllipsisButton(buttonRect, "Add...", "Add (buildIndex " + addIndex + ")",
                            EditorStyles.miniButtonLeft,
                            tooltipMsg))
                        BuildUtils.AddBuildScene(buildScene);
                    buttonRect.width /= 2;
                    buttonRect.x += buttonRect.width;
                }
                // In build settings
                else
                {
                    var isEnabled = buildScene.scene.enabled;
                    var stateString = isEnabled ? "Disable" : "Enable";
                    tooltipMsg = stateString + " this scene in build settings.\n" +
                                 (isEnabled
                                     ? "It will no longer be included in builds"
                                     : "It will be included in builds") + "." +
                                 readOnlyWarning;

                    if (EllipsisButton(buttonRect, stateString, stateString + " In Build", EditorStyles.miniButtonLeft,
                            tooltipMsg))
                        BuildUtils.SetBuildSceneState(buildScene, !isEnabled);
                    buttonRect.x += buttonRect.width;

                    tooltipMsg =
                        "Completely remove this scene from build settings.\nYou will need to add it again for it to be included in builds!" +
                        readOnlyWarning;
                    if (EllipsisButton(buttonRect, "Remove...", "Remove from Build", EditorStyles.miniButtonMid,
                            tooltipMsg))
                        BuildUtils.RemoveBuildScene(buildScene);
                }
            }

            buttonRect.x += buttonRect.width;

            tooltipMsg = "Open the 'Build Settings' Window for managing scenes." + readOnlyWarning;
            if (EllipsisButton(buttonRect, "Settings", "Build Settings", EditorStyles.miniButtonRight, tooltipMsg))
            {
                BuildUtils.OpenBuildSettings();
            }
        }

        public bool EllipsisButton(Rect position, string msgShort, string msgLong, GUIStyle style,
            string tooltip = null)
        {
            var content = new GUIContent(msgLong) {tooltip = tooltip};

            var longWidth = style.CalcSize(content).x;
            if (longWidth > position.width) content.text = msgShort;

            return GUI.Button(position, content, style);
        }
    }
}