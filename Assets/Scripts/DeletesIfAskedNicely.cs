using UnityEngine;

[ExecuteInEditMode]
public class DeletesIfAskedNicely : MonoBehaviour
{
    private bool delete = false;
    public bool switchedMeshes = false;
    public Mesh otherMesh;

    // Update is called once per frame
    void Update()
    {
        if (delete)
        {
            DestroyImmediate(gameObject);
        }
    }

    public void DeletePls()
    {
        delete = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void SetOtherMesh()
    {
        if (otherMesh == null) return;

        gameObject.GetComponent<MeshFilter>().mesh = otherMesh;
        switchedMeshes = true;
    }
}
