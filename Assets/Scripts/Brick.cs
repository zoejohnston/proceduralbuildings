using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Brick : MonoBehaviour
{
    private bool delete = false;

    public float splitNoise = 0.0f;
    public float splitLocation = 0.0f;
    public bool shouldntSplit = false;

    public List<Vector3> horizontalMins = new List<Vector3>();
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    public List<Vector3> verticalMins = new List<Vector3>();
    public List<Vector3> verticalMaxes = new List<Vector3>();
    public int collisionCount = 0;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < collisionCount; i++) {
            float horizontalMin = transform.InverseTransformPoint(horizontalMins[i]).z;
            float horizontalMax = transform.InverseTransformPoint(horizontalMaxes[i]).z;
            float verticalMin = transform.InverseTransformPoint(verticalMins[i]).y;
            float verticalMax = transform.InverseTransformPoint(verticalMaxes[i]).y;

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            HandleBrickCollisions(horizontalMin, horizontalMax, verticalMin, verticalMax);
        }

        horizontalMins = new List<Vector3>();
        horizontalMaxes = new List<Vector3>();
        verticalMins = new List<Vector3>();
        verticalMaxes = new List<Vector3>();
        collisionCount = 0;

        if (splitNoise > 0.55f && (!shouldntSplit) && (!delete)) {
            Split(splitLocation);
            shouldntSplit = true;
        }

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

    public void QueueBrickCollisions(Vector3 horizontalMin, Vector3 horizontalMax, Vector3 verticalMin, Vector3 verticalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        verticalMins.Add(verticalMin);
        verticalMaxes.Add(verticalMax);
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

    public void ResizeTop(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, (p / 2.0f) - 0.25f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, multiplier * transform.localScale.y, transform.localScale.z);
        shouldntSplit = true;
    }

    public void ResizeBottom(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, (p / 2.0f) + 0.25f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, multiplier * transform.localScale.y, transform.localScale.z);
        shouldntSplit = true;
    }

    public void Split(float p)
    {
        GameObject newBrickObject = Instantiate(gameObject);
        Brick newBrick = newBrickObject.GetComponent<Brick>();
        newBrickObject.transform.SetParent(transform.parent, false);
        newBrick.ResizeBottom(p);

        ResizeTop(p);
    }

    private Brick CopyBrick(GameObject parentObject)
    {
        GameObject newBrickObject = Instantiate(parentObject);
        Brick newBrick = newBrickObject.GetComponent<Brick>();
        newBrickObject.transform.SetParent(parentObject.transform.parent, false);

        return newBrick;
    }

    private void HandleBrickCollisions(float horizontalMin, float horizontalMax, float verticalMin, float verticalMax)
    {
        // If the brick is entirely within the vertical bounds of the collider
        if (-0.5f > verticalMin && 0.5f < verticalMax) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) { 
                DeletePls(); 

            } else if (horizontalMax > 0.5f && 0.5f > horizontalMin) {
                ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f && -0.5f < horizontalMax) {
                ResizeRight(horizontalMax);

            } else if (0.5f > horizontalMax && -0.5f < horizontalMin) {
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeLeft(horizontalMin);
                ResizeRight(horizontalMax);
            }
        // If the top of the brick is within the vertical bounds of the collider
        } else if (-0.5f > verticalMin && -0.5f < verticalMax) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) { 

            } else if (horizontalMax > 0.5f && 0.5f > horizontalMin) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeTop(verticalMax);
                newBrick.ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f && -0.5f < horizontalMax) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeTop(verticalMax);
                newBrick.ResizeRight(horizontalMax);

            } else if (0.5f > horizontalMax && -0.5f < horizontalMin) {
                Brick newBrickLeft = CopyBrick(gameObject);
                newBrickLeft.ResizeTop(verticalMax);
                newBrickLeft.ResizeRight(horizontalMax);
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeTop(verticalMax);
                newBrickRight.ResizeLeft(horizontalMin);
            }

            ResizeBottom(verticalMax); 
        // If the bottom of the brick is within the vertical bounds of the collider
        } else if (0.5f < verticalMax && 0.5f > verticalMin) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) {  

            } else if (horizontalMax > 0.5f && 0.5f > horizontalMin) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeBottom(verticalMin);
                newBrick.ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f && -0.5f < horizontalMax) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeBottom(verticalMin);
                newBrick.ResizeRight(horizontalMax);

            } else if (0.5f > horizontalMax && -0.5f < horizontalMin) {
                Brick newBrickLeft = CopyBrick(gameObject);
                newBrickLeft.ResizeBottom(verticalMin);
                newBrickLeft.ResizeRight(horizontalMax);
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeBottom(verticalMin);
                newBrickRight.ResizeLeft(horizontalMin);
            }

            ResizeTop(verticalMin);
        // If the collider is entirely within the vertical bounds of the brick
        } else if (0.5f > verticalMax && -0.5f < verticalMin) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) {  
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeBottom(verticalMax);
                ResizeTop(verticalMin);

            } else if (horizontalMax > 0.5f && 0.5f > horizontalMin) {
                Brick newBrickTop = CopyBrick(gameObject);
                newBrickTop.ResizeBottom(verticalMax);
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeLeft(horizontalMin);
                newBrickRight.ResizeTop(verticalMax);
                ResizeTop(verticalMin);
                ResizeRight(horizontalMin);

            } else if (horizontalMin < -0.5f && -0.5f < horizontalMax) {
                Brick newBrickTop = CopyBrick(gameObject);
                newBrickTop.ResizeBottom(verticalMax);
                Brick newBrickLeft = CopyBrick(gameObject);
                newBrickLeft.ResizeRight(horizontalMax);
                newBrickLeft.ResizeTop(verticalMax);
                ResizeTop(verticalMin);
                ResizeLeft(horizontalMax);

            } else if (0.5f > horizontalMax && -0.5f < horizontalMin) {
                Brick newBrickTop = CopyBrick(gameObject);
                newBrickTop.ResizeBottom(verticalMax);
                newBrickTop.ResizeRight(horizontalMin);
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeTop(verticalMax);
                newBrickRight.ResizeRight(horizontalMax);
                Brick newBrickLeft = CopyBrick(gameObject);
                newBrickLeft.ResizeBottom(verticalMin);
                newBrickLeft.ResizeLeft(horizontalMin);
                ResizeTop(verticalMin);
                ResizeLeft(horizontalMax);
            }
        }
    }
}
