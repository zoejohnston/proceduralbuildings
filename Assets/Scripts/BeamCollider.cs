using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BeamCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Beam CopyBeam(GameObject parentObject)
    {
        GameObject newBeamObject = Instantiate(parentObject);
        Beam newBeam = newBeamObject.GetComponent<Beam>();
        newBeamObject.transform.SetParent(parentObject.transform.parent, false);

        return newBeam;
    }

    void HandleBeamCollisions(GameObject parentObject, Beam beam, float horizontalMin, float horizontalMax, float verticalMin, float verticalMax)
    {
        if (-0.025f > verticalMin && 0.025f < verticalMax) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) { 
                beam.DeletePls(); 

            } else if (horizontalMax > 0.5f) {
                beam.ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f) {
                beam.ResizeRight(horizontalMax);

            } else {
                Beam newBeamRight = CopyBeam(parentObject);
                newBeamRight.ResizeLeft(horizontalMin);
                beam.ResizeRight(horizontalMax);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root != gameObject.transform.root) return;

        GameObject parentObject = gameObject.transform.parent.gameObject;
        Beam beam = parentObject.GetComponent<Beam>();

        if (other.gameObject.transform.parent.gameObject.GetComponent<Window>()) {
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();

            Vector3 horizontalDirection = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceHorizontalDirection = window.transform.TransformDirection(horizontalDirection);
            float horizontalMin = transform.InverseTransformPoint(window.transform.position - worldSpaceHorizontalDirection).z;
            float horizontalMax = transform.InverseTransformPoint(window.transform.position + worldSpaceHorizontalDirection).z;

            Vector3 verticalDirection = new Vector3(0.0f, window.transform.GetChild(0).localScale.y / 2.0f, 0.0f);
            Vector3 worldSpaceVerticalDirection = window.transform.TransformDirection(verticalDirection);
            float verticalMin = transform.InverseTransformPoint(window.transform.position - worldSpaceVerticalDirection).y;
            float verticalMax = transform.InverseTransformPoint(window.transform.position + worldSpaceVerticalDirection).y;

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            HandleBeamCollisions(parentObject, beam, horizontalMin, horizontalMax, verticalMin, verticalMax);
        }
    }
}
