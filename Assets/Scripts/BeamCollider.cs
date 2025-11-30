using UnityEngine;

/// <summary>
/// Handles collisions with this collider's parent Beam
/// </summary>
[ExecuteInEditMode]
public class BeamCollider : MonoBehaviour
{   
    // Called when a collider enters this one
    void OnTriggerEnter(Collider other)
    {
        // If not part of the same BuildingPart, ignore
        if (other.gameObject.transform.root != gameObject.transform.root) return;

        GameObject parentObject = gameObject.transform.parent.gameObject;
        Beam beam = parentObject.GetComponent<Beam>();

        // If colliding object is a Window, queue changes to this collider's parent Beam
        if (other.gameObject.transform.parent.gameObject.GetComponent<Window>()) {
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();
            Vector3 horizontalDirection = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceHorizontalDirection = window.transform.TransformDirection(horizontalDirection);

            beam.QueueBeamCollisions(
                window.transform.position - worldSpaceHorizontalDirection, 
                window.transform.position + worldSpaceHorizontalDirection
            );
        }
    }
}
