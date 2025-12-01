using UnityEngine;

/// <summary>
/// Handles collisions with this collider's parent Brick
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class BrickCollider : MonoBehaviour
{
    // Called when a collider enters this one
    void OnTriggerEnter(Collider other)
    {
        // If not part of the same BuildingPart, ignore
        if (other.gameObject.transform.root != gameObject.transform.root) return;

        GameObject parentObject = gameObject.transform.parent.gameObject;
        Brick brick = parentObject.GetComponent<Brick>();

        // If colliding object is a Quoin or a Window, queue changes to this collider's parent Brick
        if (other.gameObject.transform.parent.gameObject.GetComponent<Quoin>()) {
            Quoin quoin = other.gameObject.transform.parent.gameObject.GetComponent<Quoin>();
            Vector3 horizontalDirection = brick.transform.TransformDirection(Vector3.forward);
            Vector3 quoinSpaceHorizontalDirection = quoin.transform.InverseTransformDirection(horizontalDirection);
            Vector3 verticalDirection = brick.transform.TransformDirection(Vector3.up);

            float horizontalScale = Vector3.Dot(quoinSpaceHorizontalDirection, quoin.transform.localScale) / 2.0f;
            float verticalScale = Vector3.Dot(verticalDirection, quoin.transform.localScale) / 2.0f;

            Vector3 worldSpaceHorizontalDirection = horizontalScale * horizontalDirection;
            Vector3 worldSpaceVerticalDirection = verticalScale * verticalDirection;

            brick.QueueBrickCollisions(
                quoin.transform.position - worldSpaceHorizontalDirection,
                quoin.transform.position + worldSpaceHorizontalDirection,
                quoin.transform.position - worldSpaceVerticalDirection,
                quoin.transform.position + worldSpaceVerticalDirection
            );

        } else if (other.gameObject.transform.parent.gameObject.GetComponent<Window>()) {
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();
            Vector3 horizontalDirection = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceHorizontalDirection = window.transform.TransformDirection(horizontalDirection);
            Vector3 verticalDirection = new Vector3(0.0f, window.transform.GetChild(0).localScale.y / 2.0f, 0.0f);
            Vector3 worldSpaceVerticalDirection = window.transform.TransformDirection(verticalDirection);

            brick.QueueBrickCollisions(
                window.transform.position - worldSpaceHorizontalDirection,
                window.transform.position + worldSpaceHorizontalDirection,
                window.transform.position - worldSpaceVerticalDirection,
                window.transform.position + worldSpaceVerticalDirection
            );
        }
    }
}
