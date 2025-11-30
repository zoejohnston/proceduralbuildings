using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
public class BuildingPart : MonoBehaviour
{
    /*  Private variables */

    //private float shingleSize = 0.1f;

    [SerializeField]
    private Vector3 innerScale = Vector3.one;
    private bool scaleUpdated = false;


    /*  Public variables */

    [Header("Prefabs")]
    public Brick brick;
    public Quoin quoin;
    public Shingle shingle;
    public RidgeShingle ridgeShingle;
    public Beam beam;
    public SubBeam subBeam;
    public DeletesIfAskedNicely simpleBeam;

    [Header("Storage")]
    public GameObject brickStorage;
    public GameObject shingleStorage;
    public GameObject beamStorage;
    public GameObject windowStorage;

    [Header("Roof")]

    // A value describing the curve of the roof.
    // The height of the roof will be equal to Mathf.Pow(x, roofCurve) * roofHeight, where x is a value between 0 and 1.
    [Range(0.5f, 1.75f)]
    public float roofCurve = 1.0f;

    // The height of the roof, measured from the top of this building part's walls to the ridge at the top of its roof.
    [Range(0.1f, 2.0f)]
    public float roofHeight = 2.0f;

    // A value describing the length of the ridge at the top of the roof. The ridge becomes longer as this value decreases.
    // A value of 0.2 defaults to a fully extended ridge.
    [Range(0.2f, 1.0f)]
    public float ridgeLength = 0.5f;

    [Header("Walls")]
    // Walls are framed in wood if true. 
    public bool woodFramed = false;
    public bool plastered = false;


    /* Public functions */

    /// <summary>
    /// Called by the BuildingPart scaling tool to update the inner scaling of this building part.
    /// </summary>
    /// <param name="scaleDelta">The amount to modify the inner scaling by.</param>
    public void UpdateScale(Vector3 scaleDelta)
    {
        innerScale += scaleDelta;

        if (innerScale.x < Mathf.Epsilon) innerScale.x = Mathf.Epsilon;
        if (innerScale.y < Mathf.Epsilon) innerScale.y = Mathf.Epsilon;
        if (innerScale.z < Mathf.Epsilon) innerScale.z = Mathf.Epsilon;

        if (innerScale.x > 5.0f) innerScale.x = 5.0f;
        if (innerScale.y > 5.0f) innerScale.y = 5.0f;
        if (innerScale.z > 5.0f) innerScale.z = 5.0f;

        if (innerScale.x < 0.5f) innerScale.x = 0.5f;
        if (innerScale.y < 0.3f) innerScale.y = 0.3f;
        if (innerScale.z < 0.8f) innerScale.z = 0.8f;
        
        scaleUpdated = true;
        transform.GetChild(0).localScale = innerScale;
    }

    /// <summary>
    /// Returns the inner scale of this building part.
    /// </summary>
    public Vector3 GetScale()
    {
        return innerScale;
    }

    /// <summary>
    /// Calling function lets this building part know that it should rebuild itself next frame.
    /// </summary>
    public void UpdateNextFrame()
    {
        scaleUpdated = true;
    }

    /// <summary>
    /// Returns the point at which a given dormer would attach to the roof.
    /// </summary>
    /// <param name="startPosition">The position of the top of the window.</param>
    /// <param name="direction">A vector pointing from the window towards the center of the building part.</param>
    /// <param name="flip">The sides of the roof parallel to the ridge of the roof are often at a different angle than 
    /// those orthogonal to it. This parameter indicates if the window is up against the orthogonal portion.</param>
    public Vector3 GetDormerAttachPoint(Vector3 startPosition, Vector3 direction, bool flip)
    {
        float topOfWall = transform.position.y + (innerScale.y / 2.0f);
        float yShift = (startPosition.y - topOfWall) / roofHeight;
        if (yShift < 0.0f) yShift = 0.0f;
        float xShift = Mathf.Pow(yShift, 1.0f / roofCurve);


        Vector3 localDirection = transform.GetChild(0).InverseTransformDirection(direction);
        Vector3 worldPointOnPlane = transform.GetChild(0).TransformPoint(-0.5f * localDirection);

        Vector3 scaledLocalDirection = new Vector3(
            innerScale.x * localDirection.x,
            innerScale.y * localDirection.y,
            innerScale.z * localDirection.z
        );

        Vector3 scaledDirection = transform.GetChild(0).TransformDirection(scaledLocalDirection);
        Plane plane = new Plane(direction, worldPointOnPlane);

        float multiplier = 0.5f * xShift;
        if (flip) multiplier *= ridgeLength;
        multiplier += 0.01f;

        Vector3 point = plane.ClosestPointOnPlane(startPosition) + (multiplier * scaledDirection);
        return point;
    }
    
    /// <summary>
    /// Used to get the height above the wall of the window at <c>position</c>.
    /// </summary>
    /// <param name="position">The position of the window.</param>
    public float GetDormerHeight(Vector3 position)
    {
        float topOfWall = transform.position.y + (innerScale.y / 2.0f);
        return position.y - topOfWall;
    }

    /// <summary>
    /// Prepares this building part for export and returns the resulting MeshFilters.
    /// </summary>
    public MeshFilter[] PrepForExport()
    {
        Transform storageTransform = transform.Find("Storage");
        if (storageTransform == null) return null;

        ExportPrep exportPrep = storageTransform.GetComponent<ExportPrep>();
        if (exportPrep == null) return null;
        return exportPrep.Merge();
    }

    /// <summary>
    /// Returns true if and only if this building part is floating above the ground.
    /// </summary>
    public bool IsAboveGround() {
        return transform.position.y - (innerScale.y / 2.0f) > 0.01f;
    }


    /* Called by Unity Runtime */

    // Start is called before the first frame update.
    void Start() {
        InitBricks();
        InitRoof();
        if (woodFramed) InitBeams();
    }

    // Update is called once per frame.
    void Update() {
        if (transform.hasChanged) {
            // Actual scale should never change
            if (transform.localScale.x != 1.0f || transform.localScale.y != 1.0f || transform.localScale.z != 1.0f)
                Debug.LogError("Please use the scale tool provided in the BuildingPart tool context!");

            if (transform.localScale.x != 1.0f)
                transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
            if (transform.localScale.y != 1.0f)
                transform.localScale = new Vector3(transform.localScale.x, 1.0f, transform.localScale.z);
            if (transform.localScale.z != 1.0f)
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1.0f);
            
            transform.hasChanged = false;
        }

        if (scaleUpdated) {   
            Rebuild(); 
            HandleInteractions();
        }
    }
    
    // OnValidate is called when the script is loaded or a value is changed in the inspector.
    private void OnValidate() {
        UpdateNextFrame();
    }


    /* Private functions */

    /// <summary>
    /// Destroys and rebuilds the building. Should be called whenever a change is made to this building part's 
    /// parameters.
    /// </summary>
    private void Rebuild() {
        // Getting rid of all existing bricks, beams, and shingles
        
        /*if (brickStorage.TryGetComponent(out BrickPool brickPool)) {
            foreach (Transform childTransform in brickStorage.transform) {
                GameObject childObject = childTransform.gameObject;

                if (childObject.TryGetComponent(out Quoin quoin)) brickPool.AddToQuoinPool(quoin);
                if (childObject.TryGetComponent(out Brick brick)) brickPool.AddToBrickPool(brick);
            }
        } else {*/
            foreach (Transform childTransform in brickStorage.transform) {
                GameObject childObject = childTransform.gameObject;

                if (childObject.TryGetComponent(out Quoin quoinToDelete)) quoinToDelete.DeletePls();
                if (childObject.TryGetComponent(out Brick brickToDelete)) brickToDelete.DeletePls();
            }
        //}

        foreach (Transform childTransform in shingleStorage.transform) {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent(out Shingle shingleToDelete)) shingleToDelete.DeletePls();
            if (childObject.TryGetComponent(out RidgeShingle ridgeShingleToDelete)) ridgeShingleToDelete.DeletePls();
            if (childObject.TryGetComponent(out Brick brickToDelete)) brickToDelete.DeletePls();
        }

        foreach (Transform childTransform in windowStorage.transform) {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent(out Window window)) window.UpdatePosition();
        }

        foreach (Transform childTransform in beamStorage.transform) {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent(out DeletesIfAskedNicely plsToDelete)) plsToDelete.DeletePls();
            if (childObject.TryGetComponent(out Beam beamToDelete)) beamToDelete.DeletePls();
            if (childObject.TryGetComponent(out SubBeam subBeamToDelete)) subBeamToDelete.DeletePls();
        }

        // Rebuilding
        InitBricks();
        InitRoof();
        if (woodFramed) InitBeams();

        scaleUpdated = false;
    }

    /// <summary>
    /// Steps forward the physics simulation, allowing components to detect and handle collisions.
    /// </summary>
    private void HandleInteractions() {
        Physics.simulationMode = SimulationMode.Script;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.simulationMode = SimulationMode.FixedUpdate;
    }

    /// <summary>
    /// Instantiates and places bricks according to this building part's parameters.
    /// </summary>
    private void InitBricks()
    {
        float shrink = 0.2f;
        float height = innerScale.y;

        if (!brickStorage.TryGetComponent(out BrickPool brickPool)) return;

        if (!woodFramed) {
            BrickHelpers.InitQuoins(1.0f, height, 1.0f, this);
            BrickHelpers.InitQuoins(1.0f, height, -1.0f, this);
            BrickHelpers.InitQuoins(-1.0f, height, 1.0f, this);
            BrickHelpers.InitQuoins(-1.0f, height, -1.0f, this);
        }
        
        int numBricksTall = Mathf.RoundToInt(height / 0.1f);
        float heightOfBrick = height / numBricksTall;

        float width = innerScale.x - shrink;
        int numBricksWide = Mathf.RoundToInt(width / 0.15f);
        float widthOfBrick = width / numBricksWide;

        float[,] noise = new float[numBricksTall, numBricksWide];
        BrickHelpers.SetNoise(noise, numBricksTall, numBricksWide, 0);

        BrickHelpers.BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 90.0f, shrink / 2.0f, this);
        BrickHelpers.BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 270.0f, shrink / 2.0f, this);

        float positionOnRoof = 1.0f - (((innerScale.x / 2.0f) - 0.05f) / (innerScale.x / 2.0f));
        height = innerScale.y + (Mathf.Pow(positionOnRoof, roofCurve) * roofHeight);
        numBricksTall = Mathf.RoundToInt(height / 0.1f);
        heightOfBrick = height / numBricksTall;

        width = innerScale.z - shrink;
        numBricksWide = Mathf.RoundToInt(width / 0.15f);
        widthOfBrick = width / numBricksWide;

        noise = new float[numBricksTall, numBricksWide];
        BrickHelpers.SetNoise(noise, numBricksTall, numBricksWide, 0);

        BrickHelpers.BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 0.0f, shrink / 2.0f, this);
        BrickHelpers.BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 180.0f, shrink / 2.0f, this);

        if (IsAboveGround()) {
            /*RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("BuildingPart");
            Vector3 wallScale = new Vector3(innerScale.x - 0.15f, innerScale.y, innerScale.z - 0.15f);

            for (int i = -1; i < 2; i += 2) {
                for (int j = -1; j < 2; j += 2) {
                    for (int k = -1; k < 2; k += 2) {
                        int otherK = -1 * k;
                        Vector3 corner = transform.TransformPoint(new Vector3(i * wallScale.x, -wallScale.y, j * wallScale.z) / 2.0f);
                        Vector3 otherCorner = transform.TransformPoint(new Vector3(k * i * wallScale.x, -wallScale.y, otherK * j * wallScale.z) / 2.0f);
                        Vector3 direction = otherCorner - corner;

                        if (Physics.Raycast(corner - new Vector3(0.0f, 0.001f, 0.0f), direction, out hit, direction.magnitude, mask)) { 
                            if (hit.collider.transform.root != transform) BeamHelpers.BuildSupport(corner, hit.point, this);
                        }
                    }
                }
            }*/
        }
    }

    /// <summary>
    /// Instantiates and places wooden beams according to this building part's parameters.
    /// </summary>
    private void InitBeams()
    {   
        float shrink = 0.2f;
        int numBeamsTall = Mathf.RoundToInt(innerScale.y / 0.5f);
        float heightOfBeam = innerScale.y / numBeamsTall;
        
        float width = innerScale.z - shrink;
        int numBeamsWide = Mathf.RoundToInt(width / 0.35f);
        float widthOfBeam = width / numBeamsWide;
        
        float depth = innerScale.x - shrink;
        int numBeamsDeep = Mathf.RoundToInt(depth / 0.35f);
        float depthOfBeam = depth / numBeamsDeep;

        BeamHelpers.PlaceVerticalBeams(numBeamsTall, heightOfBeam, numBeamsWide - 1, widthOfBeam, width, innerScale.x - shrink, false, 1.0f, this);
        BeamHelpers.PlaceVerticalBeams(numBeamsTall, heightOfBeam, numBeamsDeep - 1, depthOfBeam, innerScale.x - shrink, width, true, 1.0f, this);
        BeamHelpers.PlaceVerticalBeams(numBeamsTall, heightOfBeam, numBeamsWide - 1, widthOfBeam, width, innerScale.x - shrink, false, -1.0f, this);
        BeamHelpers.PlaceVerticalBeams(numBeamsTall, heightOfBeam, numBeamsDeep - 1, depthOfBeam, innerScale.x - shrink, width, true, -1.0f, this);

        BeamHelpers.PlaceHorizontalBeams(numBeamsTall - 1, heightOfBeam, width, innerScale.x - shrink, false, 1.0f, this);
        BeamHelpers.PlaceHorizontalBeams(numBeamsTall - 1, heightOfBeam, innerScale.x - shrink, width, true, 1.0f, this);
        BeamHelpers.PlaceHorizontalBeams(numBeamsTall - 1, heightOfBeam, width, innerScale.x - shrink, false, -1.0f, this);
        BeamHelpers.PlaceHorizontalBeams(numBeamsTall - 1, heightOfBeam, innerScale.x - shrink, width, true, -1.0f, this);

        BeamHelpers.PlaceCornerBeams(innerScale.y, width, innerScale.x - shrink, this);

        if (IsAboveGround()) {
            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("BuildingPart");
            Vector3 wallScale = new Vector3(innerScale.x - 0.15f, innerScale.y, innerScale.z - 0.15f);

            for (int i = -1; i < 2; i += 2) {
                for (int j = -1; j < 2; j += 2) {
                    Vector3 corner = transform.TransformPoint(new Vector3(i * wallScale.x, -wallScale.y, j * wallScale.z) / 2.0f);
                    bool wasAbleToSupport = false;

                    if (Physics.CheckSphere(corner - new Vector3(0.0f, 0.001f, 0.0f), 0.00001f, mask)) continue;

                    for (int k = -1; k < 2; k += 2) {
                        int otherK = -1 * k;
                        Vector3 otherCorner = transform.TransformPoint(new Vector3(k * i * wallScale.x, -wallScale.y, otherK * j * wallScale.z) / 2.0f);
                        Vector3 direction = otherCorner - corner;

                        if (Physics.Raycast(corner - new Vector3(0.0f, 0.001f, 0.0f), direction, out hit, 0.3f, mask)) { 
                            BeamHelpers.BuildSupport(corner, hit.point, this);
                            wasAbleToSupport = true;
                        }
                    }

                    if (!wasAbleToSupport) {
                        BeamHelpers.BuildSignificantSupport(corner - new Vector3(0.0f, 0.001f, 0.0f), i, j, this);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Instantiates and places roof shingles according to this building part's parameters.
    /// </summary>
    private void InitRoof()
    {
        float ridgeAmount = innerScale.z / 2.0f;

        if (ridgeLength > 0.2f) {
            ridgeAmount *= ridgeLength;
            ShingleHelpers.PlaceShingles(innerScale.z, innerScale.x / 2.0f, ridgeAmount, false, 1.0f, this);
            ShingleHelpers.PlaceShingles(innerScale.z, innerScale.x / 2.0f, ridgeAmount, false, -1.0f, this);
            ShingleHelpers.PlaceShingles(innerScale.x, ridgeAmount, innerScale.x / 2.0f, true, 1.0f, this);
            ShingleHelpers.PlaceShingles(innerScale.x, ridgeAmount, innerScale.x / 2.0f, true, -1.0f, this);
            ShingleHelpers.PlaceTopRidgeShingles(ridgeLength, this);
        } else {
            ShingleHelpers.PlaceShinglesSimple(innerScale.z, innerScale.x / 2.0f, false, 1.0f, this);
            ShingleHelpers.PlaceShinglesSimple(innerScale.z, innerScale.x / 2.0f, false, -1.0f, this);
            ShingleHelpers.PlaceTopRidgeShingles(0.0f, this);
            BrickHelpers.MoreBricks(innerScale.x, 1.0f, this);
            BrickHelpers.MoreBricks(innerScale.x, -1.0f, this);

            if (!woodFramed) {
                BrickHelpers.MoreQuoins(1.0f, 1.0f, this);
                BrickHelpers.MoreQuoins(1.0f, -1.0f, this);
                BrickHelpers.MoreQuoins(-1.0f, 1.0f, this);
                BrickHelpers.MoreQuoins(-1.0f, -1.0f, this);
            }
        }
    }
}
