using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using UnityEngine;

public class AttributeTester : MonoBehaviour
{
    public SerializableType Type;

    [Header("Layer")]
    [Layer]
    public int Layer = 0;

    [Layer(true)]
    public int MaskLayer = 0;

    [Header("Navmesh")]

    [NavMeshArea]
    public int NavMeshArea = 0;

    [NavMeshArea(true)]
    public int MaskNavMeshArea = 0;

    [ContextMenu("Print Values")]
    public void PrintValues()
    {
        Debug.Log($"Layer: {Layer}");
        Debug.Log($"MaskLayer: {MaskLayer}");
        Debug.Log($"NavMeshArea: {NavMeshArea}");
        Debug.Log($"MaskNavMeshArea: {MaskNavMeshArea}");
    }
}