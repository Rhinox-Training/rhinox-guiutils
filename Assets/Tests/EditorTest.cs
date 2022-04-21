using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class GenericTest : ToggleableList<string> {}
[Serializable]
public class GenericTest2 : PairList<string, string> {}
[Serializable]
public class GenericTest3 : CustomCollection<string> {}

public class EditorTest : MonoBehaviour
{
    public Shader Shader;
    public ShaderParameterType Type;

    [ShaderParameterSelector(nameof(Shader), nameof(Type))]
    public string ShaderParam;

    [DrawAsUnityGeneric]
    public GenericTest TestList;
    [DrawAsUnityGeneric]
    public ToggleableList<int> TestList2;
    
    [DrawAsUnityGeneric]
    public GenericTest2 Test;

    [DrawAsUnityGeneric]
    public GenericTest3 StringTest;
}