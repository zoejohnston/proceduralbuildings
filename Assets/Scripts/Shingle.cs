using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements the behaviour expected of a Shingle.
/// </summary>
[ExecuteInEditMode]
public class Shingle : MonoBehaviour
{
    // Setting this to true will let the Shingle know to delete itself on the next frame
    [HideInInspector]
    private bool delete = false;

    // Extra meshes used when this shingle is right on the edge of the roof
    public Mesh rightCornerMesh;
    public Mesh leftCornerMesh;

    // Used to keep track of updates to the Shingle that need to happen on the next frame
    // See QueueShingleCollisions() for more info
    [HideInInspector]
    public List<Vector3> horizontalMins = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    [HideInInspector]
    public int collisionCount = 0;

    // Update is called once per frame
    void LateUpdate()
    {
        // Handle queued collisions
        for (int i = 0; i < collisionCount; i++) {
            float horizontalMin = transform.InverseTransformPoint(horizontalMins[i]).x;
            float horizontalMax = transform.InverseTransformPoint(horizontalMaxes[i]).x;

            HandleShingleCollisions(horizontalMin, horizontalMax);
        }

        horizontalMins = new List<Vector3>();
        horizontalMaxes = new List<Vector3>();
        collisionCount = 0;

        // Delete if needed
        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    // Disables collisions for this Shingle
    public void DisableCollisions()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// Lets this Shingle know to delete itself on the next frame update.
    /// </summary>
    public void DeletePls()
    {
        delete = true;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Queues changes to this Shingle caused by collisions. 
    /// </summary>
    /// <param name="horizontalMin">Used to describe the size and location of the object this Shingle collided with.
    /// This parameter stores a point representing the lefthand side of the collider. </param>
    /// <param name="horizontalMax">Used to describe the size and location of the object this Shingle collided with.
    /// This parameter stores a point representing the righthand side of the collider. </param>
    public void QueueShingleCollisions(Vector3 horizontalMin, Vector3 horizontalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        collisionCount++;
    }

    /// <summary>
    /// Switches to a shingle mesh that has its left corner cut off. 
    /// </summary>
    public void SwitchToLeftCornerMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = leftCornerMesh;
    }

    /// <summary>
    /// Switches to a shingle mesh that has its right corner cut off. 
    /// </summary>
    public void SwitchToRightCornerMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = rightCornerMesh;
    }

    /// <summary>
    /// Handles a collision with this shingle. Resizes the shingle according to horizontalMin and horizontalMax.
    /// </summary>
    /// <param name="horizontalMin">The z coordinate of the lefthand side of the collider in local space.</param>
    /// <param name="horizontalMax">The z coordinate of the righthand side of the collider in local space.</param>
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

    /// <summary>
    /// Scales and translates this shingle so that the righthand side is unaffected but the lefthand side is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the lefthand side by.</param>
    public void ResizeLeft(float p)
    {
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3((p / 2.0f) - 0.25f, 0.0f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(multiplier * transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    /// <summary>
    /// Scales and translates this shingle so that the lefthand side is unaffected but the righthand side is moved inward by <c>p</c>.
    /// </summary>
    /// <param name="p">The amount to move the righthand side by.</param>
    public void ResizeRight(float p)
    {
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3((p / 2.0f) + 0.25f, 0.0f, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(multiplier * transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
