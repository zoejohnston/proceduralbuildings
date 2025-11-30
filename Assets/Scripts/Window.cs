using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
public class Window : MonoBehaviour
{
    // Windows keep track of the BuildingPart they are attached to and which wall they are on
    public BuildingPart attachedBuildingPart;
    public WallCollider wall;

    // Some useful prefabs
    public Shingle shingle;
    public RidgeShingle ridgeShingle;

    // Used to set how far into the wall the window should sit
    public float offset;

    // We only want to update attachedBuildingPart if the window's localPosition changes
    private Vector3 previousPosition;
    
    // Start is called before the first frame update.
    void Start() {
        previousPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        ClearDormer();

        if (attachedBuildingPart == null) return;
        if (wall == null) return;

        if (transform.hasChanged) {
            if (previousPosition != transform.localPosition) attachedBuildingPart.UpdateNextFrame();
            previousPosition = transform.localPosition;
            transform.hasChanged = false;
        }

        if (IsHalfWay()) {
            Vector3 up = new Vector3(0.0f, (transform.GetChild(0).localScale.y / 2.0f) + 0.02f, 0.0f);
            Vector3 backwards = transform.TransformDirection(Vector3.left);
            Vector3 startPosition = transform.position + transform.TransformDirection(up);
            Vector3 endPosition = attachedBuildingPart.GetDormerAttachPoint(startPosition + (0.15f * Vector3.up), backwards, wall.connectedWall != null);
            float verticalDistanceToWall = attachedBuildingPart.GetDormerHeight(startPosition);

            Vector3 localStartPosition = transform.InverseTransformPoint(startPosition);
            Vector3 localEndPosition = transform.InverseTransformPoint(endPosition);
            
            BuildDormerTop(localStartPosition, localEndPosition);
            BuildDormerSide(localStartPosition, localEndPosition, verticalDistanceToWall, 1.0f);
            BuildDormerSide(localStartPosition, localEndPosition, verticalDistanceToWall, -1.0f);
            BuildDormerRidge(localStartPosition, localEndPosition, 1.0f);
            BuildDormerRidge(localStartPosition, localEndPosition, -1.0f);
        }
    }

    // This function is called when the Window will be destroyed
    void OnDestroy()
    {
        attachedBuildingPart.UpdateNextFrame();
    }

    public void UpdatePosition()
    {   
        if (wall == null) return;

        Vector3 localDirection = transform.InverseTransformDirection(wall.transform.position - transform.position);
        Vector3 localWallNormal = transform.InverseTransformDirection(wall.transform.TransformDirection(wall.normal));
        Vector3 trans = Vector3.Dot(localWallNormal, localDirection) * localWallNormal;

        transform.Translate(trans, Space.Self);
        transform.Translate(new Vector3(0.05f + offset, 0.0f, 0.0f), Space.Self);

        transform.hasChanged = false;
    }

    private void ClearDormer()
    {
        foreach (Transform childTransform in transform.GetChild(3))
        {
            GameObject childObject = childTransform.gameObject;
            if (childObject.TryGetComponent<Shingle>(out Shingle shingle)) shingle.DeletePls();
            if (childObject.TryGetComponent<RidgeShingle>(out RidgeShingle ridgeShingle)) ridgeShingle.DeletePls();
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
                newShingle.DisableCollisions();

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

    private void BuildDormerSide(Vector3 localStartPosition, Vector3 localEndPosition, float verticalDistanceToWall, float flip)
    {
        Vector3 direction = localEndPosition - localStartPosition;

        float length = Vector3.Distance(localStartPosition, localEndPosition);
        float width = transform.GetChild(0).localScale.z;
        float height = verticalDistanceToWall + 0.08f;
        int numShinglesLong = Mathf.RoundToInt(length / 0.07f);
        float lengthOfShingle = length / numShinglesLong;

        for (int i = 0; i < numShinglesLong; i++)
        {
            Shingle newShingle = Instantiate(shingle);
            newShingle.DisableCollisions();

            newShingle.transform.position = new Vector3(
                localStartPosition.x - (i * lengthOfShingle),
                localStartPosition.y - (height / 2.0f) + 0.02f,
                localStartPosition.z - flip * ((width / 2.0f) + 0.01f)
            );

            newShingle.transform.SetParent(transform.GetChild(3), false);
            newShingle.transform.Translate(new Vector3(0.0f, (i / (float)numShinglesLong) * direction.y, 0.0f), Space.Self);
            if (flip > 0.0f) newShingle.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
            newShingle.transform.localScale = new Vector3(lengthOfShingle, height, 0.25f);

            width -= 0.01f;
        }
    }
    
    private void BuildDormerRidge(Vector3 localStartPosition, Vector3 localEndPosition, float flip)
    {
        Vector3 direction = localEndPosition - localStartPosition;

        float length = Vector3.Distance(localStartPosition, localEndPosition);
        float horizontalLength = Vector2.Distance(
            new Vector2(localStartPosition.x, localStartPosition.z), 
            new Vector2(localEndPosition.x, localEndPosition.z)
        );
        float width = transform.GetChild(0).localScale.z;
        int numShinglesLong = Mathf.RoundToInt(length / 0.1f);
        float lengthOfShingle = length / numShinglesLong;
        float lengthOfShingleHorizontally = horizontalLength / numShinglesLong;
        float angle = Vector2.SignedAngle(Vector2.up, direction);

        for (int i = 0; i < numShinglesLong; i++)
        {
            RidgeShingle newShingle = Instantiate(ridgeShingle);

            newShingle.transform.position = new Vector3(
                localStartPosition.x - (i * lengthOfShingleHorizontally),
                localStartPosition.y + 0.015f,
                localStartPosition.z - flip * ((width / 2.0f) + 0.01f)
            );

            newShingle.transform.SetParent(transform.GetChild(3), false);
            newShingle.transform.Translate(new Vector3(0.0f, (i / (float)numShinglesLong) * direction.y, 0.0f), Space.Self);
            newShingle.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
            newShingle.transform.Rotate(new Vector3(-8.0f-angle, 0.0f, 0.0f), Space.Self);
            newShingle.transform.Rotate(new Vector3(0.0f, 0.0f, flip * 3.0f), Space.Self);
            newShingle.transform.localScale = new Vector3(0.14f, 2.0f * lengthOfShingle, 0.2f);

            width -= 0.01f;
        }
    }

    public bool IsHalfWay()
    {
        if (wall == null) return false;
        if (wall.isTopWall) return false;

        Vector3 direction = new Vector3(0.0f, transform.GetChild(0).localScale.y / 2.0f, 0.0f);
        Vector3 worldSpaceDirection = transform.TransformDirection(direction);
        Vector3 topPoint = transform.position + worldSpaceDirection;
        Vector3 bottomPoint = transform.position - worldSpaceDirection;

        return wall.PointIsWithinWall(bottomPoint) && !wall.PointIsWithinWall(topPoint);
    }

    public void SetSnap(BuildingPart buildingPart)
    {
        attachedBuildingPart = buildingPart;
    }
}
