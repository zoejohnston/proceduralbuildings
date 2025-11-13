using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WindowCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       /*if (transform.hasChanged)
        {
            Physics.simulationMode = SimulationMode.Script;
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.simulationMode = SimulationMode.FixedUpdate;

            transform.hasChanged = false;
        }*/
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WallCollider>())
        {
            Debug.Log(other.gameObject.name);
            Window window = transform.parent.gameObject.GetComponent<Window>();
            window.wall = other.gameObject.GetComponent<WallCollider>();
        }

        if (other.transform.parent.gameObject.GetComponent<BuildingPart>())
        {
            BuildingPart snappedTo = other.transform.parent.gameObject.GetComponent<BuildingPart>();
            Window window = transform.parent.gameObject.GetComponent<Window>();
            window.SetSnap(snappedTo);
            
            /*Debug.Log("lock to wall...");
            Debug.Log(snappedTo.transform.position);
            Debug.Log(snappedTo.transform.localRotation);
            Debug.Log(snappedTo.transform.localScale);*/
        }
    }
}
