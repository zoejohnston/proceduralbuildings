using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BrickCollider : MonoBehaviour
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
        GameObject parentObject = gameObject.transform.parent.gameObject;
        Brick parentBrick = parentObject.GetComponent<Brick>();
        parentBrick.OnTriggerEnter(other);
    }
}
