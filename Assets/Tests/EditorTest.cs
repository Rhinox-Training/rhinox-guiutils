using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Collections;
using UnityEngine;

[Serializable]
public class GenericTest : ToggleableList<string> {}

public class EditorTest : MonoBehaviour
{
    public Shader Shader;
    public ShaderParameterType Type;

    [ShaderParameterSelector(nameof(Shader), nameof(Type))]
    public string ShaderParam;

    public GenericTest TestList;
}