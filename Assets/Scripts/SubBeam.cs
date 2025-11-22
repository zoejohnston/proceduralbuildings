using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SubBeam : MonoBehaviour
{
    private bool delete = false;
    public bool wasMovedByCollision = false;
    public bool isRoofBeam = false;

    public List<Vector3> horizontalMins = new List<Vector3>();
    public List<Vector3> horizontalMaxes = new List<Vector3>();
    public List<Vector3> verticalMins = new List<Vector3>();
    public List<Vector3> verticalMaxes = new List<Vector3>();
    public int collisionCount = 0;

    public DeletesIfAskedNicely rightCrossBeam;
    public DeletesIfAskedNicely leftCrossBeam;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < collisionCount; i++) {
            float horizontalMin = transform.InverseTransformPoint(horizontalMins[i]).y;
            float horizontalMax = transform.InverseTransformPoint(horizontalMaxes[i]).y;
            float verticalMin = transform.InverseTransformPoint(verticalMins[i]).z;
            float verticalMax = transform.InverseTransformPoint(verticalMaxes[i]).z;

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            Vector3 windowBase = verticalMins[i];
            HandleBeamCollisions(horizontalMin, horizontalMax, verticalMin, verticalMax, windowBase, collisionCount == 1);
        }

        horizontalMins = new List<Vector3>();
        horizontalMaxes = new List<Vector3>();
        verticalMins = new List<Vector3>();
        verticalMaxes = new List<Vector3>();
        collisionCount = 0;

        if (!isRoofBeam) TryAddCrossBeams();

        if (delete) {
            DestroyImmediate(gameObject);
        }
    }

    public void TryAddCrossBeams()
    {
        if (rightCrossBeam != null) return;
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Windows");
        
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, 0.5f)) { 
            Transform parentTransform = hit.collider.gameObject.transform.parent;

            if (parentTransform.gameObject.TryGetComponent(out Beam beam))
            {
                BuildingPart buildingPart = transform.root.gameObject.GetComponent<BuildingPart>();
                float y = transform.TransformPoint(new Vector3(0.0f, 0.0f, -0.5f)).y;

                Vector3 start = new Vector3(parentTransform.position.x, y, parentTransform.position.z);
                Vector3 end = transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.5f));
                Vector3 startSlightlyBack = transform.TransformPoint(transform.InverseTransformPoint(start) - (0.01f * Vector3.left));

                Vector3 rayDirection = transform.InverseTransformDirection(end - start);
                rayDirection.x = 0.0f;

                if (Physics.Raycast(startSlightlyBack, transform.TransformDirection(rayDirection), out hit, Vector3.Distance(start, end), mask)) {
                } else {
                    rightCrossBeam = CrossBeam(buildingPart, start, end, 0.3f);
                }
            }
        }

        if (leftCrossBeam != null) return;
        
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 0.5f)) { 
            Transform parentTransform = hit.collider.gameObject.transform.parent;

            if (parentTransform.gameObject.TryGetComponent(out Beam beam))
            {
                BuildingPart buildingPart = transform.root.gameObject.GetComponent<BuildingPart>();
                float y = transform.TransformPoint(new Vector3(0.0f, 0.0f, -0.5f)).y;

                Vector3 start = new Vector3(parentTransform.position.x, y, parentTransform.position.z);
                Vector3 end = transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.5f));
                Vector3 startSlightlyBack = transform.TransformPoint(transform.InverseTransformPoint(start) - (0.01f * Vector3.left));

                Vector3 rayDirection = transform.InverseTransformDirection(end - start);
                rayDirection.x = 0.0f;

                if (Physics.Raycast(startSlightlyBack, transform.TransformDirection(rayDirection), out hit, Vector3.Distance(start, end), mask)) {
                } else {
                    leftCrossBeam = CrossBeam(buildingPart, start, end, 0.3f);
                }
            }
        }
    }

    public void QueueBeamCollisions(Vector3 horizontalMin, Vector3 horizontalMax, Vector3 verticalMin, Vector3 verticalMax)
    {
        horizontalMins.Add(horizontalMin);
        horizontalMaxes.Add(horizontalMax);
        verticalMins.Add(verticalMin);
        verticalMaxes.Add(verticalMax);
        collisionCount++;
    }

    public void DeletePls()
    {
        if (rightCrossBeam != null) rightCrossBeam.DeletePls();
        if (leftCrossBeam != null) leftCrossBeam.DeletePls();
        
        delete = true;
        gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    void HandleBeamCollisions(float horizontalMin, float horizontalMax, float verticalMin, float verticalMax, Vector3 windowBase, bool doCrossBeams)
    {
        if (wasMovedByCollision || isRoofBeam) {
            if (-0.025f > horizontalMin && 0.025f < horizontalMax) {
                if (verticalMax < -0.5f && verticalMin > 0.5f) { 
                    DeletePls(); 

                } else if (verticalMax < -0.5f) {
                    ResizeTop(verticalMin);

                } else if (verticalMin > 0.5f) {
                    ResizeBottom(verticalMax);

                } else {
                    SubBeam newBeamTop = CopyBeam(gameObject);
                    newBeamTop.ResizeBottom(verticalMax);
                    ResizeTop(verticalMin);
                }
            }
        } else {
            if (-0.025f > horizontalMin && 0.025f < horizontalMax) {
                SubBeam newBeamLeft = CopyBeam(gameObject);
                bool newBeamHasNoCollisions = newBeamLeft.MoveTo(horizontalMax + 0.02f);
                bool beamHasNoCollisions = MoveTo(horizontalMin - 0.02f);

                doCrossBeams = doCrossBeams && newBeamHasNoCollisions && beamHasNoCollisions;

                if (!doCrossBeams) return;
                if (windowBase.y < newBeamLeft.transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.5f)).y) return;

                BuildingPart buildingPart = transform.root.gameObject.GetComponent<BuildingPart>();
                DeletesIfAskedNicely crossBeam = Instantiate(buildingPart.simpleBeam);
                crossBeam.SetOtherMesh();

                crossBeam.transform.localScale = new Vector3(0.3f, 0.75f, Mathf.Abs(horizontalMax - horizontalMin) - 0.04f);

                crossBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
                Vector3 xzPosition = Vector3.Lerp(newBeamLeft.transform.position, transform.position, 0.5f);
                crossBeam.transform.position = new Vector3(xzPosition.x, windowBase.y, xzPosition.z);
                crossBeam.transform.rotation = transform.rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f);

                CrossBeam(
                    buildingPart, 
                    crossBeam.transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.5f)), 
                    newBeamLeft.transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.5f)),
                    0.3f
                );

                CrossBeam(
                    buildingPart, 
                    crossBeam.transform.TransformPoint(new Vector3(0.0f, 0.0f, -0.5f)), 
                    transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.5f)),
                    0.25f
                );
            }
        }
    }

    public void ResizeBottom(float p)
    {
        p -= 0.00001f;
        float multiplier = Mathf.Abs(p + 0.5f);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) - 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    public void ResizeTop(float p)
    {
        p += 0.00001f;
        float multiplier = Mathf.Abs(0.5f - p);
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, 0.0f, (p / 2.0f) + 0.25f));
        transform.Translate(new_point - transform.position, Space.World);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, multiplier * transform.localScale.z);
    }

    public bool MoveTo(float p)
    {
        RaycastHit hit;
        Vector3 new_point = transform.TransformPoint(new Vector3(0.0f, p, 0.0f));
        transform.Translate(new_point - transform.position, Space.World);
        wasMovedByCollision = true;
        bool shouldGetCrossBeams = true;

        Vector3 startPoint = transform.TransformPoint(new Vector3(-0.025f, 0.0f, -0.5f));
        Vector3 endPoint = transform.TransformPoint(new Vector3(-0.025f, 0.0f, 0.5f));
        LayerMask mask = LayerMask.GetMask("Windows", "Shingles");
        
        if (Physics.Raycast(startPoint, endPoint - startPoint, out hit, Vector3.Distance(startPoint, endPoint), mask)) {
            if (!isRoofBeam) ResizeBottom(transform.InverseTransformPoint(hit.point).z);
            shouldGetCrossBeams = false;
        }

        startPoint = transform.TransformPoint(new Vector3(-0.025f, 0.0f, 0.5f));
        endPoint = transform.TransformPoint(new Vector3(-0.025f, 0.0f, -0.5f));
        
        if (Physics.Raycast(startPoint, endPoint - startPoint, out hit, Vector3.Distance(startPoint, endPoint), mask)) {
            ResizeTop(transform.InverseTransformPoint(hit.point).z);
            shouldGetCrossBeams = false;
        }

        return shouldGetCrossBeams;
    }

    private SubBeam CopyBeam(GameObject parentObject)
    {
        GameObject newBeamObject = Instantiate(parentObject);
        SubBeam newBeam = newBeamObject.GetComponent<SubBeam>();
        newBeamObject.transform.SetParent(parentObject.transform.parent, false);

        return newBeam;
    }

    DeletesIfAskedNicely CrossBeam(BuildingPart buildingPart, Vector3 start, Vector3 end, float xScale)
    {
        if (start.y < end.y) return null;

        DeletesIfAskedNicely crossBeam = Instantiate(buildingPart.simpleBeam);
        crossBeam.SetOtherMesh();

        crossBeam.transform.localScale = new Vector3(xScale, 0.6f, Vector3.Distance(start, end));

        crossBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
        crossBeam.transform.position = Vector3.Lerp(start, end, 0.5f);
        crossBeam.transform.LookAt(start);
        return crossBeam;
    }
}
