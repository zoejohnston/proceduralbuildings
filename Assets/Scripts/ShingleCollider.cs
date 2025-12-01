using UnityEngine;

/// <summary>
/// Handles collisions with this collider's parent Shingle
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class ShingleCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // If not part of the same BuildingPart, ignore
        if (other.gameObject.transform.root != gameObject.transform.root) return;
        
        // If the colliding object is a window, queue collisions to be handled next frame
        if (other.gameObject.transform.parent.gameObject.GetComponent<Window>())
        {
            Shingle shingle = transform.parent.gameObject.GetComponent<Shingle>();
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();

            if (!window.IsHalfWay()) return;

            Vector3 direction = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceDirection = window.transform.TransformDirection(direction);

            shingle.QueueShingleCollisions(window.transform.position - worldSpaceDirection, window.transform.position + worldSpaceDirection);
        }
    }
}
