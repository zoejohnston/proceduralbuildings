using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Shingle : MonoBehaviour
{
    private bool delete = false;

    public Mesh rightCornerMesh;
    public Mesh leftCornerMesh;

    public List<Vector3> horizontalMins = new List<Vector3>();
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    public int collisionCount = 0;

    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 0; i < collisionCount; i++) {
            float horizontalMin = transform.InverseTransformPoint(horizontalMins[i]).x;
            float horizontalMax = transform.InverseTransformPoint(horizontalMaxes[i]).x;

            HandleShingleCollisions(horizontalMin, horizontalMax);
        }

        horizontalMins = new List<Vector3>();
        horizontalMaxes = new List<Vector3>();
        collisionCount = 0;

        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    public void DisableCollisions()
    {
        transform.GetChild(0).gameObject.SetActive(false);;
    }

    public void DeletePls()
    {
        delete = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void QueueShingleCollisions(Vector3 horizontalMin, Vector3 horizontalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        collisionCount++;
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

    private void HandleShingleCollisions(float horizontalMin, float horizontalMax)
    {
        if (horizontalMax > horizontalMin) {
            if (horizontalMin < -0.5f && 0.5f < horizontalMax) { DeletePls(); }
            else if (horizontalMin < -0.5f) { ResizeRight(horizontalMax); }
            else if (0.5f < horizontalMax) { ResizeLeft(horizontalMin); }
        } else {
            if (horizontalMin > 0.5f && -0.5f > horizontalMax) { DeletePls(); } 
            else if (horizontalMin > 0.5f) { ResizeLeft(horizontalMax); }
            else if (-0.5f > horizontalMax) { ResizeRight(horizontalMin); }
        }
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
