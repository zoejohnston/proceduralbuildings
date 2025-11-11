using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Brick : MonoBehaviour
{
    private Mesh mesh;
    private bool delete = false;
    private bool checkCollisions = false;

    // Start is called before the first frame update
    void Start()
    {
        //MeshFilter meshFilter = GetComponent<MeshFilter>();
        //mesh = meshFilter.sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.y < 0.01f || transform.localScale.z < 0.01f) DeletePls();

        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    public void DeletePls()
    {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
    
    public void EnableCollisions()
    {
        checkCollisions = true;
    }

    public void ResizeLeft(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) - 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    public void ResizeRight(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) + 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    public void ResizeTop(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, (p / 2.0f) - 0.25f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, multiplier * transform.localScale.y, transform.localScale.z);
    }

    public void ResizeBottom(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, (p / 2.0f) + 0.25f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, multiplier * transform.localScale.y, transform.localScale.z);
    }

    public void Split(float p)
    {
        GameObject newBrickObject = Instantiate(gameObject);
        Brick newBrick = newBrickObject.GetComponent<Brick>();
        newBrick.ResizeBottom(p);
        newBrick.EnableCollisions();
        newBrickObject.transform.SetParent(transform.parent);
        newBrick.transform.SetParent(transform.parent);

        ResizeTop(p);
    }

    public void OnTriggerEnter(Collider other)
    {
        if ((!checkCollisions) || !other.gameObject.transform.parent)// || other.gameObject.transform.parent != transform.parent)
        {
            return;
        }
    }
}
