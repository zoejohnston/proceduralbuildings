using UnityEngine;

/// <summary>
/// Supports specific wall behavior.
/// </summary>
public class WallCollider : MonoBehaviour
{
    public Vector3 normal;
    public WallCollider connectedWall;
    public bool isTopWall = false;

    /// <summary>
    /// Attaches a window to this wall, centered at <c>point</c>.
    /// </summary>
    public void PlaceWindow(Window window, BuildingPart buildingPart, Vector3 point)
    {
        Window newWindow = Instantiate(window, buildingPart.windowStorage.transform);
        newWindow.SetSnap(buildingPart);

        if (isTopWall) {
            newWindow.wall = connectedWall;
        } else {
            newWindow.wall = this;
        }

        newWindow.transform.position = point;
        newWindow.transform.LookAt(point + transform.TransformDirection(normal), Vector3.up);
        newWindow.transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
        newWindow.transform.Translate(new Vector3(newWindow.offset, 0.0f, 0.0f), Space.Self);
    }

    /// <summary>
    /// Returns true if <c>point</c> is inside <c>collider</c> (assumes that collider is not more than 2.0f thick).
    /// </summary>
    private bool RaycastHelper(MeshCollider collider, Vector3 point)
    {
        Ray ray = new Ray(point - normal, normal);
        RaycastHit hit;

        return collider.Raycast(ray, out hit, 2.0f);
    }
    
    /// <summary>
    /// Returns true if <c>point</c> is inside this WallCollider.
    /// </summary>
    public bool PointIsWithinWall(Vector3 point)
    {   
        // If this wall has a BoxCollider
        if (gameObject.TryGetComponent(out BoxCollider boxCollider)) {
            Vector3 closestPoint = boxCollider.ClosestPoint(point);

            if (closestPoint == point) {
                return true;
            } else {
                // If it isn't in this wall, but this wall has a connected wall, check that one too
                if (connectedWall != null) {
                    if (connectedWall.TryGetComponent(out BoxCollider connectedBoxCollider)) {
                        closestPoint = connectedBoxCollider.ClosestPoint(point);
                        if (closestPoint == point) return true;
                    } else if (connectedWall.TryGetComponent(out MeshCollider connectedMeshCollider)) {
                        if (RaycastHelper(connectedMeshCollider, point)) return true;
                    } else {
                        Debug.LogError("WallCollider requires a component of either type MeshCollider or BoxCollider");
                    }
                }
                
            }
        // If this wall has a MeshCollider
        } else if (gameObject.TryGetComponent(out MeshCollider meshCollider)) {
            if (RaycastHelper(meshCollider, point)) {
                return true;
            } else {
                // If it isn't in this wall, but this wall has a connected wall, check that one too
                if (connectedWall != null) {
                    if (connectedWall.TryGetComponent(out BoxCollider connectedBoxCollider)) {
                        Vector3 closestPoint = connectedBoxCollider.ClosestPoint(point);
                        if (closestPoint == point) return true;
                    } else if (connectedWall.TryGetComponent(out MeshCollider connectedMeshCollider)) {
                        if (RaycastHelper(connectedMeshCollider, point)) return true;
                    } else {
                        Debug.LogError("WallCollider requires a component of either type MeshCollider or BoxCollider");
                    }
                }
            }
        } else {
            Debug.LogError("WallCollider requires a component of either type MeshCollider or BoxCollider");
        }

        return false;
    }
}
