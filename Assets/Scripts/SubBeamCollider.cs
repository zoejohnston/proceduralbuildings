using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SubBeamCollider : MonoBehaviour
{   
    private SubBeam CopyBeam(GameObject parentObject)
    {
        GameObject newBeamObject = Instantiate(parentObject);
        SubBeam newBeam = newBeamObject.GetComponent<SubBeam>();
        newBeamObject.transform.SetParent(parentObject.transform.parent, false);

        return newBeam;
    }

    void HandleBeamCollisions(GameObject parentObject, SubBeam beam, float horizontalMin, float horizontalMax, float verticalMin, float verticalMax)
    {
        if (beam.wasMovedByCollision)
        {
            if (-0.025f > horizontalMin && 0.025f < horizontalMax) {
                if (verticalMax < -0.5f && verticalMin > 0.5f) { 
                    beam.DeletePls(); 

                } else if (verticalMax < -0.5f) {
                    beam.ResizeTop(verticalMin);

                } else if (verticalMin > 0.5f) {
                    beam.ResizeBottom(verticalMax);

                } else {
                    SubBeam newBeamTop = CopyBeam(parentObject);
                    newBeamTop.ResizeBottom(verticalMax);
                    beam.ResizeTop(verticalMin);
                }
            }
        } else {
            if (-0.025f > horizontalMin && 0.025f < horizontalMax) {
                SubBeam newBeamLeft = CopyBeam(parentObject);
                newBeamLeft.MoveTo(horizontalMax + 0.025f);
                beam.MoveTo(horizontalMin - 0.025f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root != gameObject.transform.root) return;

        GameObject parentObject = gameObject.transform.parent.gameObject;
        SubBeam beam = parentObject.GetComponent<SubBeam>();

        if (other.gameObject.transform.parent.gameObject.GetComponent<Window>()) {
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();

            Vector3 horizontalDirection = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceHorizontalDirection = window.transform.TransformDirection(horizontalDirection);
            float horizontalMin = transform.InverseTransformPoint(window.transform.position - worldSpaceHorizontalDirection).y;
            float horizontalMax = transform.InverseTransformPoint(window.transform.position + worldSpaceHorizontalDirection).y;

            Vector3 verticalDirection = new Vector3(0.0f, window.transform.GetChild(0).localScale.y / 2.0f, 0.0f);
            Vector3 worldSpaceVerticalDirection = window.transform.TransformDirection(verticalDirection);
            float verticalMin = transform.InverseTransformPoint(window.transform.position - worldSpaceVerticalDirection).z;
            float verticalMax = transform.InverseTransformPoint(window.transform.position + worldSpaceVerticalDirection).z;

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            HandleBeamCollisions(parentObject, beam, horizontalMin, horizontalMax, verticalMin, verticalMax);
        }
    }
}
