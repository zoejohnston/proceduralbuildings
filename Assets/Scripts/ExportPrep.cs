using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ExportPrep : MonoBehaviour
{
    // TODO
    // https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html
    private MeshFilter MergeHelper(Transform storageTransform)
    {
        MeshFilter[] meshFilters = storageTransform.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] instances = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            var meshFilter = meshFilters[i];
            if (!meshFilter.gameObject.activeInHierarchy) continue;
            
            instances[i] = new CombineInstance {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            meshFilter.gameObject.SetActive(false);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(instances);

        if (!storageTransform.gameObject.TryGetComponent<MeshFilter>(out MeshFilter storageMeshFilter)) {
            storageMeshFilter = storageTransform.gameObject.AddComponent<MeshFilter>();
        }

        storageMeshFilter.sharedMesh = combinedMesh;
        return storageMeshFilter;
    }

    public MeshFilter[] Merge()
    {
        MeshFilter[] meshFilters = new MeshFilter[3];

        Transform brickStorage = transform.Find("Bricks");
        Transform shingleStorage = transform.Find("Shingles");
        Transform beamStorage = transform.Find("Beams");

        meshFilters[0] = MergeHelper(brickStorage);
        meshFilters[1] = MergeHelper(shingleStorage);
        meshFilters[2] = MergeHelper(beamStorage);

        return meshFilters;
    }
}
