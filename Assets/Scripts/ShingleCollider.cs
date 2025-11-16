using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class ShingleCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.root != gameObject.transform.root) {
            return;
        }
        
        // If the colliding object is a window
        if (other.gameObject.transform.parent.gameObject.GetComponent<Window>())
        {
            // 
            Shingle shingle = transform.parent.gameObject.GetComponent<Shingle>();
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();

            Vector3 direction = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceDirection = window.transform.TransformDirection(direction);
            float min = transform.InverseTransformPoint(window.transform.position - worldSpaceDirection).x;
            float max = transform.InverseTransformPoint(window.transform.position + worldSpaceDirection).x;

            // 
            if (window.IsHalfWay()) {
                if (max > min) {
                    if (min < -0.5f && 0.5f < max) { shingle.DeletePls(); }
                    else if (min < -0.5f) { shingle.ResizeRight(max); }
                    else if (0.5f < max) { shingle.ResizeLeft(min); }
                } else {
                    if (min > 0.5f && -0.5f > max) { shingle.DeletePls();
                    } else if (min > 0.5f) { shingle.ResizeLeft(max); }
                    else if (-0.5f > max) { shingle.ResizeRight(min); }
                }
            }
        }
    }
}
