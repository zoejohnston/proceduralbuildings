using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ExportPrep : MonoBehaviour
{
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
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(instances);

        if (!storageTransform.gameObject.TryGetComponent<MeshFilter>(out MeshFilter storageMeshFilter)) {
            storageMeshFilter = storageTransform.gameObject.AddComponent<MeshFilter>();
        }

        storageMeshFilter.sharedMesh = combinedMesh;
        return storageMeshFilter;
    }

    //
    private void HandleWindows(Transform windowStorageTransform, MeshFilter shingles, MeshFilter beams)
    {
        foreach (Transform windowTransform in windowStorageTransform)
        {
            // TODO: Add pane export logic
            Transform frameTransform = transform.Find("Frame");
            Transform paneTransform = transform.Find("Pane");
            Transform dormerTransform = transform.Find("Dormer");

            // Merges window frame with other wood beams
            if (frameTransform.gameObject.TryGetComponent(out MeshFilter frameMeshFilter))
            {
                CombineInstance[] beamInstances = new CombineInstance[2];

                beamInstances[0] = new CombineInstance {
                    mesh = frameMeshFilter.sharedMesh,
                    transform = frameMeshFilter.transform.localToWorldMatrix,
                };
                
                beamInstances[1] = new CombineInstance {
                    mesh = beams.sharedMesh,
                    transform = Matrix4x4.identity,
                };

                Mesh combinedBeamMesh = new Mesh();
                combinedBeamMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combinedBeamMesh.CombineMeshes(beamInstances);

                beams.sharedMesh = combinedBeamMesh;
            }
            
            // Merges dormer with other shingles
            MeshFilter[] ShingleMeshFilters = dormerTransform.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] shingleInstances = new CombineInstance[ShingleMeshFilters.Length + 1];

            for (int i = 0; i < ShingleMeshFilters.Length; i++)
            {
                var meshFilter = ShingleMeshFilters[i];
                if (!meshFilter.gameObject.activeInHierarchy) continue;
                
                shingleInstances[i] = new CombineInstance {
                    mesh = meshFilter.sharedMesh,
                    transform = meshFilter.transform.localToWorldMatrix,
                };
            }

            shingleInstances[ShingleMeshFilters.Length] = new CombineInstance {
                mesh = shingles.sharedMesh,
                transform = Matrix4x4.identity,
            };

            Mesh combinedShingleMesh = new Mesh();
            combinedShingleMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedShingleMesh.CombineMeshes(shingleInstances);

            shingles.sharedMesh = combinedShingleMesh;
        }
    }

    //
    public MeshFilter[] Merge()
    {
        MeshFilter[] meshFilters = new MeshFilter[3];

        Transform brickStorage = transform.Find("Bricks");
        Transform shingleStorage = transform.Find("Shingles");
        Transform beamStorage = transform.Find("Beams");
        Transform windowStorage = transform.Find("Windows");

        meshFilters[0] = MergeHelper(brickStorage);
        meshFilters[1] = MergeHelper(shingleStorage);
        meshFilters[2] = MergeHelper(beamStorage);

        HandleWindows(windowStorage, meshFilters[1], meshFilters[2]);

        return meshFilters;
    }
}
