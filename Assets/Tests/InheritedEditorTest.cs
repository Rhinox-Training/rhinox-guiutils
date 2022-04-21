using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Rhinox.GUIUtils.Editor;
#endif

public class InheritedEditorTest : EditorTest
{
    [ShaderParameterSelector(nameof(Shader))]
    public string ShaderParam2;
}

#if UNITY_EDITOR
// [CustomEditor(typeof(InheritedEditorTest))]
// public class InheritedEditorTestEditor : DefaultEditorExtender<InheritedEditorTest>
[CustomEditor(typeof(EditorTest))] 
public class EditorTestEditor : DefaultEditorExtender<EditorTest>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
            Debug.Log($"{Target.TestList.Count}, {Target.TestList2.Count}");
        if (GUILayout.Button("Set Dirty"))
            EditorUtility.SetDirty(Target.gameObject);

        if (GUILayout.Button("Add"))
        {
            Target.TestList.Add(new Toggleable<string>("Test"));
            Target.TestList2.Add(new Toggleable<int>(2));

        }
    }
}
#endif
