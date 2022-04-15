using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Rhinox.GUIUtils.Editor;
#endif

public class InheritedEditorTest : EditorTest
{
    [ShaderParameterSelector(nameof(Shader))]
    public int ShaderParam2;
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
            Debug.Log("It works!");
            
    }
}
#endif
