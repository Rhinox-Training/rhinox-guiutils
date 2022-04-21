using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class EditorTest : MonoBehaviour
{
    public Shader Shader;
    public ShaderParameterType Type;

    [ShaderParameterSelector(nameof(Shader), nameof(Type))]
    public string ShaderParam;

    // [SerializeReference]
    public ToggleableList<string> TestList;
    
    // [SerializeReference]
    public ToggleableList<int> TestList2;
    
    public PairList<string, bool> Test;

    [SerializeReference]
    public CustomCollection<string> StringTest;
}