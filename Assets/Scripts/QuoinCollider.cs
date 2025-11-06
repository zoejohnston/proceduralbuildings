using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class QuoinCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        //BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        //Debug.DrawLine(boxCollider.bounds.max, boxCollider.bounds.min, Color.red, 0.1f, false);
    }
}
