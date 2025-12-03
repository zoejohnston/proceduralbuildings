using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements the behaviour expected of a wooden beam.
/// </summary>
[ExecuteInEditMode]
public class Beam : MonoBehaviour
{
    // Setting this to true will let the Beam know to delete itself on the next frame
    [HideInInspector]
    private bool delete = false;

    // Used to keep track of updates to the beam that need to happen on the next frame
    // See QueueBeamCollisions() for more info
    [HideInInspector]
    public List<Vector3> horizontalMins = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    [HideInInspector]
    public int collisionCount = 0;

    // LateUpdate is called once per frame
    void LateUpdate()
    {
        // Handle queued collisions
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

        // Delete if needed
        if (delete) DestroyImmediate(gameObject);
    }

    /// <summary>
    /// Lets this Beam know to delete itself on the next frame update and hides the beam from view.
    /// </summary>
    public void DeletePls()
    {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Queues changes to this Beam caused by collisions. 
    /// </summary>
    /// <param name="horizontalMin">Used to describe the size and location of the object this Beam collided with.
    /// This parameter stores a point representing the lefthand side of the collider. </param>
    /// <param name="horizontalMax">Used to describe the size and location of the object this Beam collided with.
    /// This parameter stores a point representing the righthand side of the collider. </param>
    public void QueueBeamCollisions(Vector3 horizontalMin, Vector3 horizontalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        collisionCount++;
    }

    /// <summary>
    /// Initializes and returns a copy of <c>parentObject</c>.
    /// </summary>
    /// <param name="parentObject">The object to copy.</param>
    private Beam CopyBeam(GameObject parentObject)
    {
        GameObject newBeamObject = Instantiate(parentObject);
        Beam newBeam = newBeamObject.GetComponent<Beam>();
        newBeamObject.transform.SetParent(parentObject.transform.parent, false);

        return newBeam;
    }

    /// <summary>
    /// Handles a collision with this Beam. Resizes the beam according to horizontalMin and horizontalMax.
    /// </summary>
    /// <param name="parentObject">If the beam needs to be split in two, this is the object to copy.</param>
    /// <param name="horizontalMin">The z coordinate of the lefthand side of the collider in local space.</param>
    /// <param name="horizontalMax">The z coordinate of the righthand side of the collider in local space.</param>
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

    /// <summary>
    /// Scales and translates this Beam so that the righhand side is unaffected but the lefthand side is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the lefthand side by.</param>
    private void ResizeLeft(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) - 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    /// <summary>
    /// Scales and translates this Beam so that the lefthand side is unaffected but the righhand side is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the righthand side by.</param>
    private void ResizeRight(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) + 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }
}
