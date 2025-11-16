using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SubBeam : MonoBehaviour
{
    private bool delete = false;
    public bool wasMovedByCollision = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    public void DeletePls()
    {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void ResizeBottom(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) - 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    public void ResizeTop(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) + 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    public void MoveTo(float p)
    {
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, p, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        wasMovedByCollision = true;
    }
}
