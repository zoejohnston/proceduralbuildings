using UnityEngine;

/// <summary>
/// Implements the behaviour expected of a RidgeShingle.
/// </summary>
[ExecuteInEditMode]
public class RidgeShingle : MonoBehaviour
{
    // Setting this to true will let the Quoin know to delete itself on the next frame
    private bool delete = false;
    // An alternate mesh for the ridge shingle that doesn't have an angle to it
    public Mesh flatMesh;

    // Update is called once per frame
    void Update()
    {
        if (delete) DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Lets this RidgeShingle know to delete itself on the next frame update and hides the brick from view.
    /// </summary>
    public void DeletePls()
    {
        delete = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Updates the mesh for the RidgeShingle to flatMesh.
    /// </summary>
    public void UseFlatMesh()
    {
        gameObject.GetComponent<MeshFilter>().mesh = flatMesh;
    }
}
