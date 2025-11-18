using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BeamCollider : MonoBehaviour
{   
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root != gameObject.transform.root) return;

        GameObject parentObject = gameObject.transform.parent.gameObject;
        Beam beam = parentObject.GetComponent<Beam>();

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
