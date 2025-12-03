using UnityEngine;

/// <summary>
/// Supports the wall and roof colliders of a building part.
/// </summary>
[ExecuteInEditMode]
public class BuildingPartColliders : MonoBehaviour
{
    // The base meshes used to build colliders for the building part
    [HideInInspector]
    public Mesh wallMesh;
    [HideInInspector]
    public Mesh roofRidgeMesh;
    [HideInInspector]
    public Mesh roofMesh;

    // Quick access to this component's tranform's children
    [HideInInspector]
    public GameObject wallParent;
    [HideInInspector]
    public GameObject roofParent;
    [HideInInspector]
    public GameObject mainCollider;

    // Quick and organized access to wall granchildren
    [HideInInspector]
    public GameObject[] walls;
    [HideInInspector]
    public GameObject[] wallTops;
    // Modified wall collider mesh
    [HideInInspector]
    public Mesh wallTopMesh;

    // Quick and organized access to roof granchildren
    [HideInInspector]
    public GameObject[] ridgeRoofs;
    [HideInInspector]
    public GameObject[] roofs;
    // Modified roof collider meshes
    [HideInInspector]
    public Mesh ridgeRoofMeshInternal;
    [HideInInspector]
    public Mesh roofMeshInternal;

    /// <summary>
    /// Returns all the wall colliders for this building part.
    /// </summary>
    public WallCollider[] GetWallColliders()
    {
        BuildingPart buildingPart = transform.parent.gameObject.GetComponent<BuildingPart>();
        bool noTopWalls = buildingPart.ridgeLength > 0.2f;

        WallCollider[] wallColliders = new WallCollider[noTopWalls ? 4 : 6];

        for (int i = 0; i < 4; i++) {
            wallColliders[i] = walls[i].GetComponent<WallCollider>();
        }

        if (!noTopWalls) {
            for (int i = 0; i < 2; i++) {
                wallColliders[i + 4] = wallTops[i].GetComponent<WallCollider>();
            }
        }

        return wallColliders;
    }

    /// <summary>
    /// Updates the top wall collider mesh so that it reflects the curve of the roof.
    /// </summary>
    /// <param name="buildingPart">The associated building part.</param>
    private void UpdateWallTopVertices(BuildingPart buildingPart)
    {
        Vector3[] vertices = wallMesh.vertices;

        for (var i = 0; i < vertices.Length; i++) {
            if (vertices[i].x > 0.0f) {
                float powerBase = 1.0f - (2.0f * vertices[i].x);
                vertices[i].y = Mathf.Pow(powerBase, buildingPart.roofCurve) / 2.0f;
            } else if (vertices[i].x < 0.0f) {
                float powerBase = 1.0f + (2.0f * vertices[i].x);
                vertices[i].y = Mathf.Pow(powerBase, buildingPart.roofCurve) / 2.0f;
            }
        }

        wallTopMesh.vertices = vertices;
        wallTopMesh.RecalculateBounds();
        wallTopMesh.RecalculateNormals();
        wallTops[0].GetComponent<MeshCollider>().sharedMesh = wallTopMesh;
        wallTops[1].GetComponent<MeshCollider>().sharedMesh = wallTopMesh;
    }
    
    /// <summary>
    /// Updates the roof collider mesh so that it reflects the curve of the roof. Works with
    /// UpdateRoofVertices to cover the whole roof.
    /// </summary>
    /// <param name="buildingPart">The associated building part.</param>
    private void UpdateRidgeRoofVertices(BuildingPart buildingPart)
    {
        Vector3[] vertices = roofRidgeMesh.vertices;

        for (var i = 0; i < vertices.Length; i++) {
            if (vertices[i].z > 0.0f) {
                float powerBase = 1.0f - (2.0f * vertices[i].z);
                vertices[i].y = Mathf.Pow(powerBase, buildingPart.roofCurve) / 2.0f;
            }

            if (buildingPart.ridgeLength > 0.2f) {
                float start = 1.0f - buildingPart.ridgeLength;
                float lerp = start + ((1.0f - start) * (2.0f * vertices[i].z));
                vertices[i].x = lerp * vertices[i].x;
            }
        }

        ridgeRoofMeshInternal.vertices = vertices;
        ridgeRoofMeshInternal.RecalculateBounds();
        ridgeRoofMeshInternal.RecalculateNormals();
        ridgeRoofs[0].GetComponent<MeshCollider>().sharedMesh = ridgeRoofMeshInternal;
        ridgeRoofs[1].GetComponent<MeshCollider>().sharedMesh = ridgeRoofMeshInternal;
    }
    
    /// <summary>
    /// Updates the roof collider mesh so that it reflects the curve of the roof. Works with
    /// UpdateRidgeRoofVertices to cover the whole roof.
    /// </summary>
    /// <param name="buildingPart">The associated building part.</param>
    private void UpdateRoofVertices(BuildingPart buildingPart)
    {
        Vector3[] vertices = roofMesh.vertices;

        for (var i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].z < 0.0f)
            {
                float powerBase = 1.0f + (2.0f * vertices[i].z);
                vertices[i].y = Mathf.Pow(powerBase, buildingPart.roofCurve) / 2.0f;
            }
        }

        roofMeshInternal.vertices = vertices;
        roofMeshInternal.RecalculateBounds();
        roofMeshInternal.RecalculateNormals();
        roofs[0].GetComponent<MeshCollider>().sharedMesh = roofMeshInternal;
        roofs[1].GetComponent<MeshCollider>().sharedMesh = roofMeshInternal;
    }

    /// <summary>
    /// Updates the wall and roof colliders for this building part.
    /// </summary>
    private void UpdateColliders()
    {
        BuildingPart buildingPart = transform.parent.gameObject.GetComponent<BuildingPart>();
        Vector3 buildingPartScale = buildingPart.GetScale();

        bool noTopWalls = buildingPart.ridgeLength > 0.2f;

        float xScale = buildingPartScale.x - 0.25f;
        float zScale = buildingPartScale.z - 0.25f;
        float topWallExtraHeight = noTopWalls ? 0.0f : 0.05f;

        // Main collider 
        mainCollider.transform.localScale = new Vector3(buildingPartScale.x - 0.15f, buildingPartScale.y, buildingPartScale.z - 0.15f);

        // Wall colliders
        walls[0].transform.localScale = new Vector3(xScale - 0.1f, buildingPartScale.y + topWallExtraHeight, 0.1f);
        walls[0].transform.localPosition = new Vector3(0.0f, topWallExtraHeight / 2.0f, zScale / 2.0f);

        walls[1].transform.localScale = new Vector3(0.1f, buildingPartScale.y, zScale - 0.1f);
        walls[1].transform.localPosition = new Vector3(xScale / 2.0f, 0.0f, 0.0f);

        walls[2].transform.localScale = new Vector3(xScale - 0.1f, buildingPartScale.y + topWallExtraHeight, 0.1f);
        walls[2].transform.localPosition = new Vector3(0.0f, topWallExtraHeight / 2.0f, -zScale / 2.0f);

        walls[3].transform.localScale = new Vector3(0.1f, buildingPartScale.y, zScale - 0.1f);
        walls[3].transform.localPosition = new Vector3(-xScale / 2.0f, 0.0f, 0.0f);

        if (noTopWalls) {
            wallTops[0].SetActive(false);
            wallTops[1].SetActive(false);
        } else {
            wallTops[0].SetActive(true);
            wallTops[0].transform.localScale = new Vector3(xScale - 0.1f, buildingPart.roofHeight * 2.0f, 0.1f);
            wallTops[0].transform.localPosition = new Vector3(0.0f, (buildingPartScale.y / 2.0f) + 0.05f, zScale / 2.0f);

            wallTops[1].SetActive(true);
            wallTops[1].transform.localScale = new Vector3(xScale - 0.1f, buildingPart.roofHeight * 2.0f, 0.1f);
            wallTops[1].transform.localPosition = new Vector3(0.0f, (buildingPartScale.y / 2.0f) + 0.05f, -zScale / 2.0f);
        }

        // Roof colliders
        float zModifier = noTopWalls ? 0.0f : 0.35f;

        ridgeRoofs[0].transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        ridgeRoofs[0].transform.localScale = new Vector3(buildingPartScale.z - zModifier, buildingPart.roofHeight * 2.0f, buildingPartScale.x);
        ridgeRoofs[0].transform.localPosition = new Vector3(0.0f, buildingPartScale.y / 2.0f, 0.0f);

        ridgeRoofs[1].transform.localRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
        ridgeRoofs[1].transform.localScale = new Vector3(buildingPartScale.z - zModifier, buildingPart.roofHeight * 2.0f, buildingPartScale.x);
        ridgeRoofs[1].transform.localPosition = new Vector3(0.0f, buildingPartScale.y / 2.0f, 0.0f);

        if (noTopWalls) {
            float zScaleRoof = buildingPart.ridgeLength * buildingPartScale.z;
            float halfRidgeLength = 0.5f * (1.0f - buildingPart.ridgeLength) * buildingPartScale.z;

            roofs[0].SetActive(true);
            roofs[0].transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            roofs[0].transform.localScale = new Vector3(buildingPartScale.x, buildingPart.roofHeight * 2.0f, zScaleRoof);
            roofs[0].transform.localPosition = new Vector3(0.0f, buildingPartScale.y / 2.0f, halfRidgeLength);

            roofs[1].SetActive(true);
            roofs[1].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            roofs[1].transform.localScale = new Vector3(buildingPartScale.x, buildingPart.roofHeight * 2.0f, zScaleRoof);
            roofs[1].transform.localPosition = new Vector3(0.0f, buildingPartScale.y / 2.0f, -halfRidgeLength);
        } else {
            roofs[0].SetActive(false);
            roofs[1].SetActive(false);
        }

        // Update meshes
        if (noTopWalls) {
            UpdateRoofVertices(buildingPart);
        } else {
            UpdateWallTopVertices(buildingPart); 
        }
        
        UpdateRidgeRoofVertices(buildingPart);
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateColliders();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateColliders();
    }
}
