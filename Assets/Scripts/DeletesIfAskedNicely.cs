using UnityEngine;

/// <summary>
/// A catchall component that meets a few different needs.
/// </summary>
[ExecuteInEditMode]
public class DeletesIfAskedNicely : MonoBehaviour
{
    // Setting this to true will let the WoodBlock know to delete itself on the next frame
    [HideInInspector]
    private bool delete = false;
    // Indicates if the mesh of this object has been switched
    [HideInInspector]
    public bool switchedMeshes = false;
    // Another mesh to use as needed
    public Mesh otherMesh;

    // Update is called once per frame
    void Update()
    {
        if (delete) DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Lets this know to delete itself on the next frame update.
    /// </summary>
    public void DeletePls()
    {
        delete = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Sets this object's mesh to it's secondary mesh
    /// </summary>
    public void SetOtherMesh()
    {
        if (otherMesh == null) return;

        gameObject.GetComponent<MeshFilter>().mesh = otherMesh;
        switchedMeshes = true;
    }
}
