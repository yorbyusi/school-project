using UnityEngine;

public class BlendShapeController : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;

    private Mesh _mesh;

    void Awake()
    {
        if (skinnedMeshRenderer == null)
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        _mesh = skinnedMeshRenderer.sharedMesh;
    }

    public void SetBlend(string shapeName, float value)
    {
        int index = _mesh.GetBlendShapeIndex(shapeName);
        if (index != -1)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(index, value);
        }
        else
        {
            Debug.LogWarning("Blendshape '" + shapeName + "' not found.");
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < _mesh.blendShapeCount; i++)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(i, 0);
        }
    }

    // === EXPRESSIONS ===
    public void Smile()
    {
        ClearAll();
        SetBlend("Fcl_BRW_Fun", 32);
        SetBlend("Fcl_EYE_Joy", 73);
        SetBlend("Fcl_MTH_Joy", 80);
    }

    public void Angry()
    {
        ClearAll();
        SetBlend("Fcl_ALL_Angry", 100);
        SetBlend("Fcl_BRW_Angry", 100);
        SetBlend("Fcl_EYE_Angry", 100);
        SetBlend("Fcl_MTH_Angry", 100);
    }

    public void Shy()
    {
        ClearAll();
        SetBlend("Fcl_EYE_Close", 100);
        SetBlend("Fcl_MTH_Sorrow", 40);
        SetBlend("Fcl_BRW_Sorrow", 40);
    }
}
