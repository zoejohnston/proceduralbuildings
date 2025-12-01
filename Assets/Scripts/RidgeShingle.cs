using UnityEngine;

[ExecuteInEditMode]
public class RidgeShingle : MonoBehaviour
{
    private bool delete = false;
    public Mesh flatMesh;

    // Start is called before the first frame update
    void Start()
    {

    }

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

    public void UseFlatMesh()
    {
        gameObject.GetComponent<MeshFilter>().mesh = flatMesh;
    }
}
