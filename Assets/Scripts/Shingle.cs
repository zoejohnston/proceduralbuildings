using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Shingle : MonoBehaviour
{
    private bool delete = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (delete)
        {
            DestroyImmediate(gameObject);
        }
    }

    public void DeletePls()
    {
        delete = true;
    }
}
