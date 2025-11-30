using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements the behaviour expected of a brick.
/// </summary>
[ExecuteInEditMode]
public class Brick : MonoBehaviour
{
    // Setting this to true will let the Beam know to delete itself on the next frame
    private bool delete = false;

    // Stores information that will be used to determine if this brick should be split in two for
    // aesthetic effect
    public float splitNoise = 0.0f;
    public float splitLocation = 0.0f;
    public bool shouldntSplit = true;

    // Used to keep track of updates to the beam that need to happen on the next frame
    // See QueueBeamCollisions() for more info
    public List<Vector3> horizontalMins = new List<Vector3>();
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    public List<Vector3> verticalMins = new List<Vector3>();
    public List<Vector3> verticalMaxes = new List<Vector3>();
    public int collisionCount = 0;

    // Update is called once per frame
    void LateUpdate()
    {
        // Handle queued collisions
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

        horizontalMins.Clear();
        horizontalMaxes.Clear();
        verticalMins.Clear();
        verticalMaxes.Clear();
        collisionCount = 0;

        // Split if needed
        if (splitNoise > 0.55f && (!shouldntSplit) && (!delete)) {
            Split(splitLocation);
            shouldntSplit = true;
        }

        // Delete if needed
        if (transform.localScale.y < 0.01f || transform.localScale.z < 0.01f) DeletePls();

        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// Lets this Brick know to delete itself on the next frame update and hides the brick from view.
    /// </summary>
    public void DeletePls()
    {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Queues changes to this Brick caused by collisions. 
    /// </summary>
    /// <param name="horizontalMin">Used to describe the size and location of the object this Beam collided with.
    /// This parameter stores a point representing the lefthand side of the collider. </param>
    /// <param name="horizontalMax">Used to describe the size and location of the object this Beam collided with.
    /// This parameter stores a point representing the righthand side of the collider. </param>
    /// <param name="verticalMin">Used to describe the size and location of the object this Beam collided with.
    /// This parameter stores a point representing the bottom of the collider. </param>
    /// <param name="verticalMax">Used to describe the size and location of the object this Beam collided with.
    /// This parameter stores a point representing the top of the collider. </param>
    public void QueueBrickCollisions(Vector3 horizontalMin, Vector3 horizontalMax, Vector3 verticalMin, Vector3 verticalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        verticalMins.Add(verticalMin);
        verticalMaxes.Add(verticalMax);
        collisionCount++;
    }

    /// <summary>
    /// Scales and translates this Beam so that the righhand side is unaffected but the lefthand side is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the lefthand side by.</param>
    public void ResizeLeft(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) - 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    /// <summary>
    /// Scales and translates this Beam so that the lefthand side is unaffected but the righthand side is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the righthand side by.</param>
    public void ResizeRight(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) + 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    /// <summary>
    /// Scales and translates this Beam so that the bottom is unaffected but the top is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the top by.</param>
    public void ResizeTop(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, (p / 2.0f) - 0.25f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, multiplier * transform.localScale.y, transform.localScale.z);
        shouldntSplit = true;
    }

    /// <summary>
    /// Scales and translates this Beam so that the top is unaffected but the bottom is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the bottom by.</param>
    public void ResizeBottom(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, (p / 2.0f) + 0.25f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, multiplier * transform.localScale.y, transform.localScale.z);
        shouldntSplit = true;
    }

    /// <summary>
    /// Splits this brick in two.
    /// </summary>
    /// <param name="p">Where to make the split.</param>
    public void Split(float p)
    {
        GameObject newBrickObject = Instantiate(gameObject);
        Brick newBrick = newBrickObject.GetComponent<Brick>();
        newBrickObject.transform.SetParent(transform.parent, false);
        newBrick.ResizeBottom(p);

        ResizeTop(p);
    }

    /// <summary>
    /// Initializes and returns a copy of <c>parentObject</c>.
    /// </summary>
    /// <param name="parentObject">The object to copy.</param>
    private Brick CopyBrick(GameObject parentObject)
    {
        GameObject newBrickObject = Instantiate(parentObject);
        Brick newBrick = newBrickObject.GetComponent<Brick>();
        newBrickObject.transform.SetParent(parentObject.transform.parent, false);

        return newBrick;
    }

    /// <summary>
    /// Handles a collision with this Brick. Resizes the brick according to horizontalMin, horizontalMax, verticalMin, and verticalMax.
    /// </summary>
    /// <param name="horizontalMin">The z coordinate of the lefthand side of the collider in local space.</param>
    /// <param name="horizontalMax">The z coordinate of the righthand side of the collider in local space.</param>
    /// <param name="verticalMin">The y coordinate of the bottom of the collider in local space.</param>
    /// <param name="verticalMax">The y coordinate of the top of the collider in local space.</param>
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
                ResizeBottom(verticalMax); 
            } else if (horizontalMax > 0.5f && 0.5f > horizontalMin) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeTop(verticalMax);
                newBrick.ResizeLeft(horizontalMin);
                ResizeBottom(verticalMax); 

            } else if (horizontalMin < -0.5f && -0.5f < horizontalMax) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeTop(verticalMax);
                newBrick.ResizeRight(horizontalMax);
                ResizeBottom(verticalMax); 

            } else if (0.5f > horizontalMax && -0.5f < horizontalMin) {
                Brick newBrickLeft = CopyBrick(gameObject);
                newBrickLeft.ResizeTop(verticalMax);
                newBrickLeft.ResizeRight(horizontalMax);
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeTop(verticalMax);
                newBrickRight.ResizeLeft(horizontalMin);
                ResizeBottom(verticalMax); 
            } 
        // If the bottom of the brick is within the vertical bounds of the collider
        } else if (0.5f < verticalMax && 0.5f > verticalMin) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) {  
                ResizeTop(verticalMin);
            } else if (horizontalMax > 0.5f && 0.5f > horizontalMin) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeBottom(verticalMin);
                newBrick.ResizeLeft(horizontalMin);
                ResizeTop(verticalMin);

            } else if (horizontalMin < -0.5f && -0.5f < horizontalMax) {
                Brick newBrick = CopyBrick(gameObject);
                newBrick.ResizeBottom(verticalMin);
                newBrick.ResizeRight(horizontalMax);
                ResizeTop(verticalMin);

            } else if (0.5f > horizontalMax && -0.5f < horizontalMin) {
                Brick newBrickLeft = CopyBrick(gameObject);
                newBrickLeft.ResizeBottom(verticalMin);
                newBrickLeft.ResizeRight(horizontalMax);
                Brick newBrickRight = CopyBrick(gameObject);
                newBrickRight.ResizeBottom(verticalMin);
                newBrickRight.ResizeLeft(horizontalMin);
                ResizeTop(verticalMin);
            }
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
