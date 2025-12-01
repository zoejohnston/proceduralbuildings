using UnityEngine;

/// <summary>
/// Implements the behaviour expected of a quoin.
/// </summary>
[ExecuteInEditMode]
public class Quoin : MonoBehaviour
{
    // Setting this to true will let the Quoin know to delete itself on the next frame
    private bool delete = false;

    // Update is called once per frame
    void Update()
    {
        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// Lets this Quoin know to delete itself on the next frame update and hides the brick from view.
    /// </summary>
    public void DeletePls() {
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
}
