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
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.sharedMesh;
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
        newBrickObject.transform.SetParent(transform.parent);
        newBrick.ResizeBottom(p);
        newBrick.EnableCollisions();

        ResizeTop(p);
    }

    void OnTriggerEnter(Collider other)
    {
        if ((!checkCollisions) || other.gameObject.transform.parent != transform.parent)
        {
            return;
        }

        Collider self = gameObject.GetComponent<BoxCollider>();

        if (other.gameObject.GetComponent<Quoin>())
        {
            Vector3 topLeft = transform.TransformPoint(new Vector3(0.0f, 0.5f, 0.5f));
            Vector3 topRight = transform.TransformPoint(new Vector3(0.0f, 0.5f, -0.5f));
            Vector3 bottomLeft = transform.TransformPoint(new Vector3(0.0f, -0.5f, 0.5f));
            Vector3 bottomRight = transform.TransformPoint(new Vector3(0.0f, -0.5f, -0.5f));

            bool topLeftCollision = other.bounds.Contains(topLeft);
            bool topRightCollision = other.bounds.Contains(topRight);
            bool bottomLeftCollision = other.bounds.Contains(bottomLeft);
            bool bottomRightCollision = other.bounds.Contains(bottomRight);

            bool[] cornerChecks = new bool[] { topLeftCollision, topRightCollision, bottomLeftCollision, bottomRightCollision };
            int cornerChecksFailed = cornerChecks.Count(b => b);

            /*if (name == "Brick[2,1]270")
            {
                Debug.Log("checks failed: " + cornerChecksFailed);
                Debug.Log(topLeftCollision + " " + topRightCollision + " " + bottomLeftCollision + " " + bottomRightCollision);
                Debug.Log(other.bounds.Contains(other.gameObject.transform.InverseTransformPoint(bottomLeft)));
                if (other.bounds.Contains(bottomLeft))
                {
                    Debug.Log(other.gameObject.name);
                    Debug.DrawLine(bottomLeft, other.bounds.min, Color.red, 20.0f, false);
                    Debug.DrawLine(bottomLeft, other.bounds.max, Color.red, 20.0f, false);
                }
            }*/

            if (cornerChecksFailed == 4)
            {
                delete = true;
            }
            else if (cornerChecksFailed > 1)
            {
                if (topLeftCollision && bottomLeftCollision)
                {
                    Vector3 rightPoint = new Vector3(0.0f, 0.0f, -0.5f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(rightPoint)));

                    ResizeLeft(otherPoint.z);
                }
                if (topRightCollision && bottomRightCollision)
                {
                    Vector3 leftPoint = new Vector3(0.0f, 0.0f, 0.5f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(leftPoint)));

                    ResizeRight(otherPoint.z);
                }
                if (topLeftCollision && topRightCollision)
                {
                    Vector3 bottomPoint = new Vector3(0.0f, -0.5f, 0.0f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(bottomPoint)));

                    ResizeTop(otherPoint.y);
                }
                if (bottomLeftCollision && bottomRightCollision)
                {
                    Vector3 topPoint = new Vector3(0.0f, 0.5f, 0.0f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(topPoint)));

                    ResizeBottom(otherPoint.y);
                }
            }
            else if (cornerChecksFailed == 1)
            {
                if (topLeftCollision)
                {
                    Vector3 bottomRightPoint = new Vector3(0.0f, -0.5f, -0.5f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(bottomRightPoint)));

                    checkCollisions = false;
                    GameObject newBrickObject = Instantiate(gameObject);
                    Brick newBrick = newBrickObject.GetComponent<Brick>();
                    newBrickObject.transform.SetParent(transform.parent);
                    newBrick.ResizeBottom(otherPoint.y);
                    newBrick.ResizeLeft(otherPoint.z);
                    checkCollisions = true;

                    ResizeTop(otherPoint.y);
                }
                else if (topRightCollision)
                {
                    Vector3 bottomLeftPoint = new Vector3(0.0f, -0.5f, 0.5f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(bottomLeftPoint)));

                    checkCollisions = false;
                    GameObject newBrickObject = Instantiate(gameObject);
                    Brick newBrick = newBrickObject.GetComponent<Brick>();
                    newBrickObject.transform.SetParent(transform.parent);
                    newBrick.ResizeBottom(otherPoint.y);
                    newBrick.ResizeRight(otherPoint.z);
                    checkCollisions = true;

                    ResizeTop(otherPoint.y);
                }
                else if (bottomLeftCollision)
                {
                    Vector3 topRightPoint = new Vector3(0.0f, 0.5f, -0.5f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(topRightPoint)));

                    checkCollisions = false;
                    GameObject newBrickObject = Instantiate(gameObject);
                    Brick newBrick = newBrickObject.GetComponent<Brick>();
                    newBrickObject.transform.SetParent(transform.parent);
                    newBrick.ResizeTop(otherPoint.y);
                    newBrick.ResizeLeft(otherPoint.z);
                    checkCollisions = true;

                    ResizeBottom(otherPoint.y);
                }
                else if (bottomRightCollision)
                {
                    Vector3 topLeftPoint = new Vector3(0.0f, 0.5f, 0.5f);
                    Vector3 otherPoint = transform.InverseTransformPoint(other.ClosestPoint(transform.TransformPoint(topLeftPoint)));

                    checkCollisions = false;
                    GameObject newBrickObject = Instantiate(gameObject);
                    Brick newBrick = newBrickObject.GetComponent<Brick>();
                    newBrickObject.transform.SetParent(transform.parent);
                    newBrick.ResizeTop(otherPoint.y);
                    newBrick.ResizeRight(otherPoint.z);
                    checkCollisions = true;

                    ResizeBottom(otherPoint.y);
                }
            }
        }
    }
}
