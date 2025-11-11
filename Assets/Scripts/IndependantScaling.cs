using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IndependantScaling : MonoBehaviour
{
    void SetScale()
    {
        transform.localScale = new Vector3(
            1.0f / transform.parent.localScale.x,
            1.0f / transform.parent.localScale.y,
            1.0f / transform.parent.localScale.z
        );
    }
    // Start is called before the first frame update
    void Start()
    {
        SetScale();
    }

    // Update is called once per frame
    void Update()
    {
        SetScale();
    }
}
