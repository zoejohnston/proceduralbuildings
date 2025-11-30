using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class Beam : MonoBehaviour
{
    private bool delete = false;

    public List<Vector3> horizontalMins = new List<Vector3>();
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    public int collisionCount = 0;

    // Update is called once per frame
    void LateUpdate()
    {
        for (int i = 0; i < collisionCount; i++) {
            float horizontalMin = transform.InverseTransformPoint(horizontalMins[i]).z;
            float horizontalMax = transform.InverseTransformPoint(horizontalMaxes[i]).z;

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            HandleBeamCollisions(gameObject, horizontalMin, horizontalMax);
        }

        horizontalMins = new List<Vector3>();
        horizontalMaxes = new List<Vector3>();
        collisionCount = 0;

        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    public void DeletePls()
    {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private Beam CopyBeam(GameObject parentObject)
    {
        GameObject newBeamObject = Instantiate(parentObject);
        Beam newBeam = newBeamObject.GetComponent<Beam>();
        newBeamObject.transform.SetParent(parentObject.transform.parent, false);

        return newBeam;
    }

    private void HandleBeamCollisions(GameObject parentObject, float horizontalMin, float horizontalMax)
    {
        if ((horizontalMax < -0.5f && horizontalMin < -0.5f) || (horizontalMax > 0.5f && horizontalMin > 0.5f)) {
            return;

        } else if (horizontalMax > 0.5f && horizontalMin < -0.5f) { 
            DeletePls(); 

        } else if (horizontalMax > 0.5f) {
            ResizeLeft(horizontalMin);

        } else if (horizontalMin < -0.5f) {
            ResizeRight(horizontalMax);

        } else if (0.5 > horizontalMax && -0.5f < horizontalMin) {
            Beam newBeamRight = CopyBeam(parentObject);
            newBeamRight.ResizeLeft(horizontalMin);
            ResizeRight(horizontalMax);
        }
    }

    public void QueueBeamCollisions(Vector3 horizontalMin, Vector3 horizontalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        collisionCount++;
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
}
