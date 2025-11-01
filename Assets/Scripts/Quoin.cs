using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Quoin : MonoBehaviour
{
    private bool delete = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collided with");
    }

    public void DeletePls() {
        delete = true;
    }
}
