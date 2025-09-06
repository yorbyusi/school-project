using UnityEngine;
using UnityEditor;

public class MeshTriangleChecker
{
    [MenuItem("Tools/Check Mesh Triangle Count")]
    public static void CheckTriangleCounts()
    {
        int triangleThreshold = 2000;

        // Find all MeshFilters in the scene
        MeshFilter[] meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
        SkinnedMeshRenderer[] skinnedMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();

        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            int tris = mf.sharedMesh.triangles.Length / 3;
            if (tris > triangleThreshold)
            {
                Debug.Log($"[Mesh] Tris Count: {tris}", mf.gameObject);
            }
        }

        foreach (var smr in skinnedMeshes)
        {
            if (smr.sharedMesh == null) continue;

            int tris = smr.sharedMesh.triangles.Length / 3;
            if (tris > triangleThreshold)
            {
                Debug.Log($"[SkinnedMesh] Tris Count: {tris}", smr.gameObject);
            }
        }

        Debug.Log("Mesh triangle check completed");
    }
}
