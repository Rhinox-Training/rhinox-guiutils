using System;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GUIUtils.Editor.Drawers
{
    public class UnitySupportWarningDrawer: OdinAttributeDrawer<UnitySupportWarningAttribute> 
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            base.DrawPropertyLayout(label);

            string unityVersionStr = Application.unityVersion;
            int index = unityVersionStr.IndexOf("f");
            if (index != -1)
                unityVersionStr = unityVersionStr.Substring(0, index);
            
            var currentSemanticVersion = new Version(unityVersionStr);
            var minimumSupportedVersion = new Version(Attribute.Major, Attribute.Minor, 0);

            if (minimumSupportedVersion > currentSemanticVersion)
            {
                EditorGUILayout.HelpBox($"This GUI layout contains a property which is only properly supported from version {minimumSupportedVersion.ToString()} or higher.", MessageType.Warning);
            }
        }
    }
}