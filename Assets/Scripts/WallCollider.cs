using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WallCollider : MonoBehaviour
{
    public Vector3 normal;
    public WallCollider connectedWall;
    public bool isTopWall = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

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

    private bool RaycastHelper(MeshCollider collider, Vector3 point)
    {
        Ray ray = new Ray(point - normal, normal);
        RaycastHit hit;

        return collider.Raycast(ray, out hit, 2.0f);
    }
    
    public bool PointIsWithinWall(Vector3 point)
    {
        if (gameObject.TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
        {
            Vector3 closestPoint = boxCollider.ClosestPoint(point);

            if (closestPoint == point) {
                return true;
            } else {
                if (connectedWall != null) {
                    if (connectedWall.TryGetComponent<BoxCollider>(out BoxCollider connectedBoxCollider)) {
                        closestPoint = connectedBoxCollider.ClosestPoint(point);
                        if (closestPoint == point) return true;
                    } else if (connectedWall.TryGetComponent<MeshCollider>(out MeshCollider connectedMeshCollider)) {
                        if (RaycastHelper(connectedMeshCollider, point)) return true;
                    }
                }
                
            }
        }
        else if (gameObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
        {
            if (RaycastHelper(meshCollider, point)) {
                return true;
            } else {
                if (connectedWall != null) {
                    if (connectedWall.TryGetComponent<BoxCollider>(out BoxCollider connectedBoxCollider)) {
                        Vector3 closestPoint = connectedBoxCollider.ClosestPoint(point);
                        if (closestPoint == point) return true;
                    } else if (connectedWall.TryGetComponent<MeshCollider>(out MeshCollider connectedMeshCollider)) {
                        if (RaycastHelper(connectedMeshCollider, point)) return true;
                    }
                }
            }
        }

        return false;
    }
}
