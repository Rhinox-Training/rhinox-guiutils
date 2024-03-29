using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using UnityEditor;
using UnityEngine;

[Serializable]
public class GenericTest : ToggleableList<string> {}
[Serializable]
public class GenericTest2 : PairList<string, string> {}
[Serializable]
public class GenericTest3 : CustomCollection<string> {}

[Serializable]
public class ShaderData
{
    public Shader Shader;
    [ShaderParameterSelector(nameof(Shader))]
    public string ShaderParam;
}

public interface ISerializeReferenceTester
{
    
}
[Serializable]
public class SimpleStringData : ISerializeReferenceTester
{
    public string String;
}
[Serializable]
public class SimpleNumberData : ISerializeReferenceTester
{
    public float Float;
}

[SmartFallbackDrawn]
public class EditorTest : MonoBehaviour
{
    [SerializeReference, DrawAsReference]
    public ISerializeReferenceTester SerializeReferenceTesterWithStupidlyLongName;
    
    [SerializeReference]
    public ISerializeReferenceTester SerializeReferenceNodraw;
    
    [SerializeReference, DrawAsReference]
    public ISerializeReferenceTester[] SerializeReferenceTesterArray;
    
    public ShaderData ShaderParamData;

    [ToggleButtonOpposite("Right")]
    public bool Left;

    public Shader AlsoAShader;

    public Shader Shader;
    //public ShaderParameterType Type;


    [ShaderParameterSelector(nameof(Shader), nameof(Type))]
    public string ShaderParam;

    [DrawAsUnityGeneric]
    public GenericTest TestList;
    [DrawAsUnityGeneric, Tooltip("Doesn't work pre-2020")]
    public ToggleableList<int> TestList2;
    
    [DrawAsUnityGeneric]
    public GenericTest2 Test;

    [DrawAsUnityGeneric]
    public GenericTest3 StringTest;
}