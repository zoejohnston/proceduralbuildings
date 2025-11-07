using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Shingle : MonoBehaviour
{
    private bool delete = false;
    public Mesh rightCornerMesh;
    public Mesh leftCornerMesh;
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
    }

    public void SwitchToLeftCornerMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = leftCornerMesh;
    }

    public void SwitchToRightCornerMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = rightCornerMesh;
    }
}
