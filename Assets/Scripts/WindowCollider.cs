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
       if (transform.hasChanged)
        {
            Physics.simulationMode = SimulationMode.Script;
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.simulationMode = SimulationMode.FixedUpdate;

            transform.hasChanged = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BuildingPart>())
        {
            Debug.Log("lock to wall...");
            Debug.Log(other.gameObject.transform.position);
            Debug.Log(other.gameObject.transform.localRotation);
            Debug.Log(other.gameObject.transform.localScale);
        }
    }
}
