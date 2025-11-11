using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Window : MonoBehaviour
{
    private BuildingPart snappedTo;
    public Shingle shingle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (snappedTo == null) return;
        ClearDormer();

        if (transform.hasChanged)
        {
            //snappedTo.GetWindowPosition();

            transform.hasChanged = false;
        }

        if (IsHalfWay()) {
            Vector3 up = new Vector3(0.0f, (transform.GetChild(0).localScale.y / 2.0f) + 0.02f, 0.0f);
            Vector3 backwards = transform.TransformDirection(Vector3.left);
            Vector3 startPosition = transform.position + transform.TransformDirection(up);
            Vector3 endPosition = snappedTo.GetDormerAttachPoint(startPosition + (0.15f * Vector3.up), backwards);

            Debug.DrawLine(startPosition, endPosition, Color.red);

            Vector3 localStartPosition = transform.InverseTransformPoint(startPosition);
            Vector3 localEndPosition = transform.InverseTransformPoint(endPosition);
            BuildDormerTop(localStartPosition, localEndPosition);
            BuildDormerSides(localStartPosition, localEndPosition);
        }
    }

    private void ClearDormer()
    {
        foreach (Transform childTransform in transform.GetChild(3))
        {
            GameObject childObject = childTransform.gameObject;
            if (childObject.TryGetComponent<Shingle>(out Shingle shingle)) shingle.DeletePls();
        }
    }

    private void BuildDormerTop(Vector3 localStartPosition, Vector3 localEndPosition) 
    {
        Vector3 direction = localEndPosition - localStartPosition;
        float length = Vector3.Distance(localStartPosition, localEndPosition);
        float width = transform.GetChild(0).localScale.z;

        int numShinglesLong = Mathf.RoundToInt(length / 0.1f);
        float lengthOfShingle = length / numShinglesLong;

        float angle = Vector2.SignedAngle(Vector2.up, direction);

        for (int i = 0; i < numShinglesLong; i++) {
            int numShinglesWide = Mathf.RoundToInt(width / 0.1f);
            float widthOfShingle = width / numShinglesWide;

            for (int j = 0; j < numShinglesWide; j++) {
                Shingle newShingle = Instantiate(shingle);

                newShingle.transform.position = new Vector3(
                    localStartPosition.x,
                    localStartPosition.y,
                    localStartPosition.z - (width / 2.0f) + (widthOfShingle / 2.0f) + (j * widthOfShingle)
                );

                newShingle.transform.SetParent(transform.GetChild(3), false);
                newShingle.transform.Translate((i / (float) numShinglesLong) * direction, Space.Self);
                newShingle.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
                newShingle.transform.Rotate(new Vector3(-10.0f - angle, 0.0f, 0.0f), Space.Self);
                newShingle.transform.localScale = new Vector3(widthOfShingle, 1.5f * lengthOfShingle, 0.25f);

                float zNoise = 0.025f * (Mathf.PerlinNoise(i / 0.7f, j / 0.8f) - 0.5f);
                float yNoise = -0.025f * Mathf.PerlinNoise(i / 0.3f, j / 0.97f);
                newShingle.transform.Translate(new Vector3(0.0f, yNoise, zNoise));
            }

            width -= 0.01f;
        }
    }

    private void BuildDormerSides(Vector3 localStartPosition, Vector3 localEndPosition) 
    {
        
    }

    public bool IsHalfWay()
    {
        Vector3 direction = new Vector3(0.0f, transform.GetChild(0).localScale.y / 2.0f, 0.0f);
        Vector3 worldSpaceDirection = transform.TransformDirection(direction);
        float min = (transform.position - worldSpaceDirection).y;
        float max = (transform.position + worldSpaceDirection).y;
        float topOfWall = snappedTo.transform.position.y + (snappedTo.transform.localScale.y / 2.0f);

        return max > topOfWall && min < topOfWall;
    }

    public void SetSnap(BuildingPart buildingPart)
    {
        snappedTo = buildingPart;
    }
}
