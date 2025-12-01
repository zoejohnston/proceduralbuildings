using UnityEngine;

/// <summary>
/// Handles collisions with this collider's parent SubBeam
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class SubBeamCollider : MonoBehaviour
{   
    void OnTriggerEnter(Collider other)
    {
        // If not part of the same BuildingPart, ignore
        if (other.gameObject.transform.root != gameObject.transform.root) return;

        GameObject parentObject = gameObject.transform.parent.gameObject;
        SubBeam beam = parentObject.GetComponent<SubBeam>();

        // If the colliding object is a window, queue collisions to be handled next frame
        if (other.gameObject.transform.parent.gameObject.GetComponent<Window>()) {
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();

            Vector3 horizontalDirection = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceHorizontalDirection = window.transform.TransformDirection(horizontalDirection);
            Vector3 verticalDirection = new Vector3(0.0f, window.transform.GetChild(0).localScale.y / 2.0f, 0.0f);
            Vector3 worldSpaceVerticalDirection = window.transform.TransformDirection(verticalDirection);

            beam.QueueBeamCollisions(
                window.transform.position - worldSpaceHorizontalDirection,
                window.transform.position + worldSpaceHorizontalDirection,
                window.transform.position - worldSpaceVerticalDirection,
                window.transform.position + worldSpaceVerticalDirection
            );
        }
    }
}
