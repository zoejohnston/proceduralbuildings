using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Quoin : MonoBehaviour
{
    private bool delete = false;

    // Update is called once per frame
    void Update()
    {
        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    public void DeletePls() {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
