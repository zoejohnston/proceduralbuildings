using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to get all the meshes under this building part's storage transform ready for export to FBX.
/// </summary>
[ExecuteInEditMode]
public class ExportPrep : MonoBehaviour
{
    /// <summary>
    /// Returns false if and only if every corner of <c>componentTransform</c> is inside a collider with a layer in <c>mask</c>.
    /// Used by <c>ShouldInclude</c> to determine if <c>componentTransform</c> should be included in the exported mesh or not.
    /// </summary>
    /// <param name="thisRoot">The relevant BuildingPart.</param>
    /// <param name="mask">A LayerMask defining which colliders we care about.</param>
    /// <param name="componentTransform">The Transform to check.</param>
    /// <param name="x">The local x component of the size of the <c>componentTransform</c>.</param>
    /// <param name="y">The local y component of the size of the <c>componentTransform</c>.</param>
    /// <param name="z">The local z component of the size of the <c>componentTransform</c>.</param>
    private bool ShouldIncludeHelper(Transform thisRoot, LayerMask mask, Transform componentTransform, float x, float y, float z)
    {
        int cornerCount = 0;

        for (int i = -1; i < 2; i += 2) {
            for (int j = -1; j < 2; j += 2) {
                for (int k = -1; k < 2; k += 2) {
                    Vector3 point = componentTransform.TransformPoint(new Vector3(x * i, y * j, z * k));
                    Collider[] hitColliders = Physics.OverlapSphere(point, 0.00001f, mask);

                    foreach (Collider hitCollider in hitColliders) {
                        if (hitCollider.transform.root != thisRoot) cornerCount++;
                    }
                }
            }
        }

        return cornerCount != 8;
    }

    /// <summary>
    /// Returns true if and only if <c>go</c> should be included in the exported mesh.
    /// </summary>
    /// <param name="go">The GameObject to check.</param>
    private bool ShouldInclude(GameObject go)
    {   
        LayerMask mask = LayerMask.GetMask("BuildingPart", "Roof");
        Transform thisRoot = transform.root;

        if (go.transform.parent.gameObject.TryGetComponent(out Brick brick)) {
            return ShouldIncludeHelper(thisRoot, mask, brick.transform, 0.5f, 0.5f, 0.5f);
        } else if (go.TryGetComponent(out Shingle shingle)) {
            return ShouldIncludeHelper(thisRoot, mask, shingle.transform, 0.5f, 0.5f, 0.5f);
        } else if (go.TryGetComponent(out RidgeShingle ridgeShingle)) {
            return ShouldIncludeHelper(thisRoot, mask, ridgeShingle.transform, 0.225f, 0.5f, 0.15f);
        } else if (go.transform.parent.gameObject.TryGetComponent(out Beam beam)) {
            return ShouldIncludeHelper(thisRoot, mask, beam.transform, 0.025f, 0.025f, 0.5f);
        } else if (go.transform.parent.gameObject.TryGetComponent(out SubBeam subBeam)) {
            return ShouldIncludeHelper(thisRoot, mask, subBeam.transform, 0.025f, 0.025f, 0.5f);
        } else if (go.transform.parent.gameObject.TryGetComponent(out Quoin quoin)) {
            return ShouldIncludeHelper(thisRoot, mask, quoin.transform, 0.5f, 0.5f, 0.5f);
        } else if (go.TryGetComponent(out DeletesIfAskedNicely other)) {
            if (other.switchedMeshes) {
                return ShouldIncludeHelper(thisRoot, mask, other.transform, 0.025f, 0.025f, 0.5f);
            } else {
                return ShouldIncludeHelper(thisRoot, mask, other.transform, 0.5f, 0.5f, 0.5f);
            }
        }

        return true;
    }

    /// <summary>
    /// Merges every MeshFilter in <c>storageTransform</c> into a single MeshFilter.
    /// Code adapted from: https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html
    /// </summary>
    /// <param name="storageTransform">The Transform whose children are the MeshFilters we'd like to combine.</param>
    private MeshFilter MergeHelper(Transform storageTransform)
    {
        MeshFilter[] meshFilters = storageTransform.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> listOfInstances = new List<CombineInstance>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            var meshFilter = meshFilters[i];
            if (!meshFilter.gameObject.activeInHierarchy) continue;
            if (!ShouldInclude(meshFilter.gameObject)) continue;
            
            CombineInstance newInstance = new CombineInstance {
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            listOfInstances.Add(newInstance);
        }

        CombineInstance[] instances = new CombineInstance[listOfInstances.Count];
        for (int i = 0; i < listOfInstances.Count; i++) {
            instances[i] = listOfInstances[i];
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

    /// <summary>
    /// Loops through every child window of <c>windowStorageTransform</c> and adds windows and beams found in them to 
    /// <c>shingles</c> and <c>beams</c>. Also creates and returns a new MeshFilter <c>panes</c>, which stores the windows'
    /// glass panes.
    /// </summary>
    /// <param name="windowStorageTransform">The Transform storing the BuildingPart's windows as its children.</param>
    /// <param name="shingles">The MeshFilter to save any shingles to.</param>
    /// <param name="beams">The MeshFilter to save any wooden components to.</param>
    private MeshFilter HandleWindows(Transform windowStorageTransform, MeshFilter shingles, MeshFilter beams)
    {
        MeshFilter panes = gameObject.AddComponent<MeshFilter>();

        foreach (Transform windowTransform in windowStorageTransform)
        {
            Transform frameTransform = windowTransform.Find("Frame");
            Transform paneTransform = windowTransform.Find("Pane");
            Transform dormerTransform = windowTransform.Find("Dormer");

            // Merges glass pane with others
            if (paneTransform.gameObject.TryGetComponent(out MeshFilter paneMeshFilter)) {
                if (panes.sharedMesh == null) {
                    CombineInstance[] paneInstances = new CombineInstance[1];

                    paneInstances[0] = new CombineInstance {
                        mesh = paneMeshFilter.sharedMesh,
                        transform = paneMeshFilter.transform.localToWorldMatrix,
                    };

                    Mesh combinedBeamMesh = new Mesh();
                    combinedBeamMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    combinedBeamMesh.CombineMeshes(paneInstances);

                    panes.sharedMesh = combinedBeamMesh;
                } else {
                    CombineInstance[] paneInstances = new CombineInstance[2];

                    paneInstances[0] = new CombineInstance {
                        mesh = paneMeshFilter.sharedMesh,
                        transform = paneMeshFilter.transform.localToWorldMatrix,
                    };
                    
                    paneInstances[1] = new CombineInstance {
                        mesh = panes.sharedMesh,
                        transform = Matrix4x4.identity,
                    };

                    Mesh combinedBeamMesh = new Mesh();
                    combinedBeamMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    combinedBeamMesh.CombineMeshes(paneInstances);

                    panes.sharedMesh = combinedBeamMesh;
                }
            }

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

        return panes;
    }

    /// <summary>
    /// Returns an array of MeshFilters. There is one MeshFilter for each type of material in the building.
    /// The MeshFilter at index 0 stores stone bricks, the MeshFilter at index 1 stores shingles, the MeshFilter 
    /// at index 2 stores wooden beams, and the MeshFilter at index 3 stores glass panes.
    /// </summary>
    public MeshFilter[] Merge()
    {
        MeshFilter[] meshFilters = new MeshFilter[4];

        Transform brickStorage = transform.Find("Bricks");
        Transform shingleStorage = transform.Find("Shingles");
        Transform beamStorage = transform.Find("Beams");
        Transform windowStorage = transform.Find("Windows");

        meshFilters[0] = MergeHelper(brickStorage);
        meshFilters[1] = MergeHelper(shingleStorage);
        meshFilters[2] = MergeHelper(beamStorage);
        meshFilters[3] = HandleWindows(windowStorage, meshFilters[1], meshFilters[2]);

        return meshFilters;
    }
}
