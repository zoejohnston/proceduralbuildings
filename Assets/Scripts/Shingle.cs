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
        gameObject.GetComponent<MeshRenderer>().enabled = false;
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

    public void ResizeLeft(float p)
    {
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3((p / 2.0f) - 0.25f, 0.0f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(multiplier * transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    public void ResizeRight(float p)
    {
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3((p / 2.0f) + 0.25f, 0.0f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(multiplier * transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
