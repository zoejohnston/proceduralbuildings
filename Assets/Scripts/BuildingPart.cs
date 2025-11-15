using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BuildingPart : MonoBehaviour
{
    /*  Private variables */

    private float shingleSize = 0.1f;
    private bool updatedLastFrame = false;

    [SerializeField]
    private Vector3 innerScale = Vector3.one;
    private bool scaleUpdated = false;

    /*  Public variables */

    [Header("Prefabs")]
    public Brick brick;
    public Quoin quoin;
    public Shingle shingle;
    public RidgeShingle ridgeShingle;

    [Header("Storage")]
    public GameObject brickStorage;
    public GameObject shingleStorage;
    public GameObject beamStorage;
    public GameObject windowStorage;

    [Header("Roof")]
    [Range(0.5f, 1.75f)]
    public float roofCurve = 1.0f;

    [Range(0.1f, 2.0f)]
    public float roofHeight = 2.0f;

    [Range(0.2f, 1.0f)]
    public float ridgeLength = 0.5f;

    [Header("Walls")]
    public bool woodFramed = false;
    public bool plastered = false;

    /* Public functions */

    public void UpdateScale(Vector3 scaleDelta)
    {
        innerScale += scaleDelta;

        if (innerScale.x < Mathf.Epsilon) innerScale.x = Mathf.Epsilon;
        if (innerScale.y < Mathf.Epsilon) innerScale.y = Mathf.Epsilon;
        if (innerScale.z < Mathf.Epsilon) innerScale.z = Mathf.Epsilon;

        if (innerScale.x > 10.0f) innerScale.x = 10.0f;
        if (innerScale.y > 10.0f) innerScale.y = 10.0f;
        if (innerScale.z > 10.0f) innerScale.z = 10.0f;
        
        scaleUpdated = true;
        transform.GetChild(0).localScale = innerScale;
    }

    public Vector3 GetScale()
    {
        return innerScale;
    }

    public void UpdateNextFrame()
    {
        scaleUpdated = true;
    }

    public Vector3 GetDormerAttachPoint(Vector3 startPosition, Vector3 direction)
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

        float multiplier = (0.5f * xShift) + 0.01f;

        Vector3 point = plane.ClosestPointOnPlane(startPosition) + (multiplier * scaledDirection);
        return point;
    }
    
    public float GetDormerHeight(Vector3 position)
    {
        float topOfWall = transform.position.y + (innerScale.y / 2.0f);
        return position.y - topOfWall;
    }

    /* Private functions */

    private void OnValidate()
    {
        UpdateNextFrame();
    }

    void Rebuild()
    {
        foreach (Transform childTransform in brickStorage.transform)
        {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent<Quoin>(out Quoin quoin)) quoin.DeletePls();
            if (childObject.TryGetComponent<Brick>(out Brick brick)) brick.DeletePls();
        }

        foreach (Transform childTransform in shingleStorage.transform)
        {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent<Shingle>(out Shingle shingle)) shingle.DeletePls();
            if (childObject.TryGetComponent<RidgeShingle>(out RidgeShingle ridgeShingle)) ridgeShingle.DeletePls();
            if (childObject.TryGetComponent<Brick>(out Brick brick)) brick.DeletePls();
        }

        foreach (Transform childTransform in windowStorage.transform)
        {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent<Window>(out Window window)) window.UpdatePosition();
        }

        InitBricks();
        InitRoof();
        if (woodFramed) InitBeams();

        updatedLastFrame = true;
        scaleUpdated = false;
    }

    void HandleInteractions()
    {
        Physics.simulationMode = SimulationMode.Script;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.simulationMode = SimulationMode.FixedUpdate;

        updatedLastFrame = false;
    }

    /*  Helpers  */

    void InitQuoins(float x, float y, float z)
    {
        float epsilon = 0.000001f;
        y += epsilon;

        int numBricksTall = Mathf.RoundToInt(y / 0.2f);
        float heightOfBrick = y / numBricksTall;

        float widthOfBrick = 0.15f;
        float lenthOfBrick = 0.25f;

        for (int i = 0; i < numBricksTall; i++)
        {
            float temp = widthOfBrick;
            widthOfBrick = lenthOfBrick;
            lenthOfBrick = temp;

            Quoin new_quoin = Instantiate(quoin);

            new_quoin.transform.localPosition = new Vector3(
                x * ((innerScale.x / 2.0f) - (widthOfBrick / 2.0f) - 0.05f),
                -(innerScale.y / 2.0f) + (heightOfBrick / 2.0f) + (heightOfBrick * i) - (epsilon / 2.0f),
                z * ((innerScale.z / 2.0f) - (lenthOfBrick / 2.0f) - 0.05f)
            );

            new_quoin.transform.localScale = new Vector3(widthOfBrick, heightOfBrick, lenthOfBrick);
            new_quoin.transform.SetParent(brickStorage.transform, false);
            new_quoin.name = "Quoin[" + x + "," + z + "]" + i;
        }
    }

    void SetNoise(float[,] noise, int numBricksTall, int numBricksWide)
    {
        for (int i = 0; i < numBricksTall; i++)
        {
            float sum = 0.0f;
            noise[i, 0] = Mathf.PerlinNoise(i / 0.7f, 0 / 0.7f) - 0.5f;
            sum += noise[i, 0];

            for (int j = 1; j < numBricksWide - 1; j++)
            {
                noise[i, j] = Mathf.PerlinNoise(i / 0.7f, j / 0.7f) - 0.5f;
                sum += noise[i, j];
            }

            noise[i, numBricksWide - 1] = -1.0f * sum;
        }
    }
    
    void BuildBrickWall(int numBricksTall, int numBricksWide, float heightOfBrick, float widthOfBrick, float[,] noise, float rotation, float shrink)
    {
        float direction = 1.0f;
        if (rotation > 100.0f) direction = -1.0f;
        bool swap = false;
        if (rotation < 50.0f || (100.0f < rotation && rotation < 200.0f)) swap = true;

        for (int i = 0; i < numBricksTall; i++)
        {
            float upTo = 0.0f;
            for (int j = 0; j < numBricksWide; j ++)
            {
                Brick newBrick = Instantiate(brick);
                float newWidth = Mathf.Abs(widthOfBrick + 0.1f * noise[i, j]);
                float zNoise = 0.025f * (Mathf.PerlinNoise(i / 0.8f, j / 0.8f) - 0.5f);
                
                if (swap)
                {
                    newBrick.transform.localPosition = new Vector3(
                        -direction * ((innerScale.x / 2.0f) - (0.25f / 2.0f)) + zNoise,
                        -(innerScale.y / 2.0f) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        ((innerScale.z / 2.0f) - shrink) - upTo - (newWidth / 2.0f)
                    );
                }
                else
                {
                    newBrick.transform.localPosition = new Vector3(
                        ((innerScale.x / 2.0f) - shrink) - upTo - (newWidth / 2.0f),
                        -(innerScale.y / 2.0f) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        direction * ((innerScale.z / 2.0f) - (0.25f / 2.0f)) + zNoise
                    );
                }
                
                newBrick.name = "Brick[" + i + "," + j + "]" + rotation;

                float rotationalNoise = Mathf.PerlinNoise(i / 0.87f, j / 0.87f);
                if (rotationalNoise > 0.75f)
                {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 90.0f);
                } 
                else if (rotationalNoise > 0.5f)
                {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 180.0f);
                } 
                else if (rotationalNoise > 0.25f)
                {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 270.0f);
                }

                newBrick.transform.localEulerAngles = new Vector3(0.0f, rotation, 0.0f);
                newBrick.transform.localScale = new Vector3(0.1f, heightOfBrick, newWidth);
                newBrick.transform.SetParent(brickStorage.transform, false);

                float splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                float splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);
                /*if (splitNoise > 0.65f) newBrick.Split(splitLocation);*/

                /*if (i != 0 && i != numBricksTall - 1)
                {
                    float heightDisplacement = 0.1f * (Mathf.PerlinNoise(newBrick.transform.position.x, newBrick.transform.position.z) - 0.5f);
                    newBrick.transform.Translate(new Vector3(0.0f, heightDisplacement, 0.0f), Space.World);
                }*/

                newBrick.EnableCollisions();

                upTo += newWidth;
            }
        }
    }

    private void DistanceToGround(){

    }

    void InitBricks()
    {
        float shrink = 0.2f;
        float height = innerScale.y;

        if (!woodFramed) {
            InitQuoins(1.0f, height, 1.0f);
            InitQuoins(1.0f, height, -1.0f);
            InitQuoins(-1.0f, height, 1.0f);
            InitQuoins(-1.0f, height, -1.0f);
        }
        
        // Rest of bricks
        int numBricksTall = Mathf.RoundToInt(height / 0.1f);
        float heightOfBrick = height / numBricksTall;

        float width = innerScale.x - shrink;
        int numBricksWide = Mathf.RoundToInt(width / 0.15f);
        float widthOfBrick = width / numBricksWide;

        float[,] noise = new float[numBricksTall, numBricksWide];
        SetNoise(noise, numBricksTall, numBricksWide);

        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 90.0f, shrink / 2.0f);
        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 270.0f, shrink / 2.0f);
        
        width = innerScale.z - shrink;
        numBricksWide = Mathf.RoundToInt(width / 0.15f);
        widthOfBrick = width / numBricksWide;

        noise = new float[numBricksTall, numBricksWide];
        SetNoise(noise, numBricksTall, numBricksWide);

        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 0.0f, shrink / 2.0f);
        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 180.0f, shrink / 2.0f);
    }

    private float EstimateArcLength(float xDist)
    {
        int numSteps = 20;
        float arcLength = 0.0f;
        Vector2 previousPoint = Vector2.zero;

        for (int i = 1; i <= numSteps; i++)
        {
            float x = i / (float)numSteps;
            Vector2 currentPoint = new Vector2(x * xDist, Mathf.Pow(x, roofCurve) * roofHeight);
            arcLength += Vector2.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return arcLength;
    }

    private float ShingleStep(float xShift, float yShift, float current, float lengthOfShingle, float depth)
    {
        float step = 0.0f;
        Vector2 currentPoint = new Vector2(xShift, yShift);
        Vector2 nextPoint = new Vector2((current + step) * depth, Mathf.Pow(current + step, roofCurve) * roofHeight);

        while (Vector2.Distance(currentPoint, nextPoint) < lengthOfShingle)
        {
            step += 0.001f;
            nextPoint.x = (current + step) * depth;
            nextPoint.y = Mathf.Pow(current + step, roofCurve) * roofHeight;
        }

        return step;
    }

    Vector3 PlaceRidgeShingles(float xShift, float yShift, float current, float otherWidth, float width, float side, float which, Vector3 previous, int count)
    {
        RidgeShingle newShingle = Instantiate(ridgeShingle);

        newShingle.transform.position = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            which * ((width / 2.0f) - (current * otherWidth))
        );

        float multiplier = count > 0 ? 1.2f : 0.8f;
        float zScale = count > 0 ? 0.25f : 0.14f;
        float extraTranslate = count > 0 ? 0.04f : 0.06f;
        float lengthOfShingle = multiplier * Vector3.Distance(previous, newShingle.transform.position);
        newShingle.transform.LookAt(previous, Vector3.up);

        Vector3 retVal = newShingle.transform.position;

        newShingle.transform.Translate(new Vector3(0.0f, 0.0f, (lengthOfShingle / 2.0f) + extraTranslate));
        newShingle.transform.Rotate(-90.0f - (5.5f * roofCurve), 0.0f, 0.0f, Space.Self);
        newShingle.transform.Translate(new Vector3(0.0f, 0.0f, 0.01f), Space.Self);
        newShingle.transform.localScale = new Vector3(0.15f, lengthOfShingle, zScale);
        newShingle.transform.SetParent(shingleStorage.transform, false);

        return retVal;
    }

    void PlaceTopRidgeShingles(float length)
    {
        float width = ((1.0f - length) * innerScale.z) + 0.09f;
        int numShinglesWide = Mathf.RoundToInt(width / 0.12f);
        float widthOfShingle = width / numShinglesWide;

        if (numShinglesWide == 0)
        {
            numShinglesWide = 1;
            widthOfShingle = 0.09f;
        }

        for (int i = 0; i < numShinglesWide; i++)
        {
            RidgeShingle newShingle = Instantiate(ridgeShingle);
            float zScale = 0.35f;
            float yScale = 0.01f;
            float zTrans = 0.01f;
            float tilt = 10.0f;

            if (i > 0 && i < numShinglesWide - 1) {
                newShingle.UseFlatMesh();
                zScale = 0.2f;
                yScale = 0.0f;
                zTrans = 0.04f;
                tilt = 0.0f;
            }

            newShingle.transform.localPosition = new Vector3(
                0.0f,
                (innerScale.y / 2.0f) + roofHeight - 0.02f,
                (width / 2.0f) - (i * widthOfShingle)
            );

            float angle = i == numShinglesWide - 1 ? 180.0f : 0.0f;
            float flip = i == numShinglesWide - 1 ? -1.0f : 1.0f;
            newShingle.transform.Translate(new Vector3(0.0f, 0.0f, widthOfShingle / -2.0f));
            newShingle.transform.Rotate(flip * tilt - 90.0f, 0.0f, angle, Space.Self);
            newShingle.transform.Translate(new Vector3(0.0f, 0.0f, zTrans - 0.02f), Space.Self);
            newShingle.transform.localScale = new Vector3(0.14f, yScale+widthOfShingle, zScale);
            newShingle.transform.SetParent(shingleStorage.transform, false);
        }
    }

    void InstantiateShingle(int index, float widthOfShingle, float lengthOfShingle, int numShinglesWide, float xShift, float yShift, float startPosition,
        float side, float angle, float tilt, bool flip)
    {
        Shingle newShingle = Instantiate(shingle);

        if (flip) {
            if (index == 0) newShingle.SwitchToRightCornerMesh();
            if (index == numShinglesWide - 1) newShingle.SwitchToLeftCornerMesh();
        } else {
            if (index == 0) newShingle.SwitchToLeftCornerMesh();
            if (index == numShinglesWide - 1) newShingle.SwitchToRightCornerMesh();
        }

        if (flip) {
            newShingle.transform.localPosition = new Vector3(
                (widthOfShingle / 2.0f) + startPosition + (index * widthOfShingle),
                (innerScale.y / 2.0f) + yShift,
                side * ((innerScale.z / 2.0f) - xShift)
            );
        } else {
            newShingle.transform.localPosition = new Vector3(
                side * ((innerScale.x / 2.0f) - xShift),
                (innerScale.y / 2.0f) + yShift,
                (widthOfShingle / 2.0f) + startPosition + (index * widthOfShingle)
            );
        }

        if (!flip) newShingle.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        Vector3 translateAmount = new Vector3(0.0f, -0.075f, 0.0f);
        Vector3 axis = flip ? new Vector3(1.0f, 0.0f, 0.0f) : new Vector3(0.0f, 0.0f, 1.0f);

        newShingle.transform.Translate(translateAmount);
        newShingle.transform.RotateAround(newShingle.transform.position - translateAmount, axis, angle);
        newShingle.transform.RotateAround(newShingle.transform.position, axis, tilt);
        newShingle.transform.localScale = new Vector3(widthOfShingle, 1.5f * lengthOfShingle, 0.25f);

        float zNoise = 0.025f * (Mathf.PerlinNoise(xShift / 0.7f, index / 0.8f) - 0.5f);
        float yNoise = -0.05f * (Mathf.PerlinNoise(xShift / 0.3f, index / 0.97f) - 0.5f);
        newShingle.transform.Translate(new Vector3(0.0f, yNoise, zNoise));

        newShingle.transform.SetParent(shingleStorage.transform, false);
    }
    
    void PlaceShingles(float width, float depth, float otherWidth, bool flip, float side)
    {
        float arcLength = EstimateArcLength(depth);
        int numShinglesTall = Mathf.RoundToInt(arcLength / shingleSize);
        float lengthOfShingle = arcLength / numShinglesTall;

        float current = Mathf.Epsilon;

        float xShift = current * depth;
        float yShift = Mathf.Pow(current, roofCurve) * roofHeight;
        Vector2 previous = new Vector2(xShift, yShift);

        Vector3 rightRidgeShingle = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            (width / 2.0f) - (current * otherWidth)
        );
        Vector3 leftRidgeShingle = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            (current * otherWidth) - (width / 2.0f)
        );

        int count = 0;

        while (current <= 1.0f + Mathf.Epsilon)
        {
            float step = ShingleStep(xShift, yShift, current, lengthOfShingle, depth);
            current += step;

            xShift = current * depth;
            yShift = Mathf.Pow(current, roofCurve) * roofHeight;

            if (!flip)
            {
                rightRidgeShingle = PlaceRidgeShingles(xShift, yShift, current, otherWidth, width, side, 1.0f, rightRidgeShingle, count);
                leftRidgeShingle = PlaceRidgeShingles(xShift, yShift, current, otherWidth, width, side, -1.0f, leftRidgeShingle, count);
                count++;
            }

            float distance = width - (2.0f * (current - step) * otherWidth);
            int numShinglesWide = Mathf.RoundToInt(distance / shingleSize);
            float widthOfShingle = distance / numShinglesWide;

            float startPosition = ((current - step) * otherWidth) - (width / 2.0f);

            Vector2 uhh = new Vector2(xShift, yShift);
            Vector2 uhhhh = uhh - previous;

            float angle = side * Vector2.SignedAngle(Vector2.up, uhhhh);
            float tilt = side * 15.0f;
            if (!flip) angle = -angle;
            if (flip) tilt = -tilt;

            for (int j = 0; j < numShinglesWide; j++)
            {
                InstantiateShingle(j, widthOfShingle, lengthOfShingle, numShinglesWide, xShift, yShift, startPosition, side, angle, tilt, flip);
            }

            previous = uhh;
        }
    }

    void PlaceShinglesSimple(float width, float depth, bool flip, float side)
    {
        float arcLength = EstimateArcLength(depth);
        int numShinglesTall = Mathf.RoundToInt(arcLength / shingleSize);
        float lengthOfShingle = arcLength / numShinglesTall;

        float current = Mathf.Epsilon;

        float xShift = current * depth;
        float yShift = Mathf.Pow(current, roofCurve) * roofHeight;
        Vector2 previous = new Vector2(xShift, yShift);

        while (current <= 1.0f + Mathf.Epsilon)
        {
            float step = ShingleStep(xShift, yShift, current, lengthOfShingle, depth);
            current += step;

            xShift = current * depth;
            yShift = Mathf.Pow(current, roofCurve) * roofHeight;

            float distance = width - 0.15f;
            int numShinglesWide = Mathf.RoundToInt(distance / shingleSize);
            float widthOfShingle = distance / numShinglesWide;

            float startPosition = -distance / 2.0f;

            Vector2 uhh = new Vector2(xShift, yShift);
            Vector2 uhhhh = uhh - previous;

            float angle = side * Vector2.SignedAngle(Vector2.up, uhhhh);
            float tilt = side * 15.0f;
            if (!flip) angle = -angle;
            if (flip) tilt = -tilt;

            for (int j = 1; j < numShinglesWide - 1; j++)
            {
                InstantiateShingle(j, widthOfShingle, lengthOfShingle, numShinglesWide, xShift, yShift, startPosition, side, angle, tilt, flip);
            }

            previous = uhh;
        }
    }

    void MoreQuoins(float x, float y, float z)
    {
        Quoin new_quoin = Instantiate(quoin);

        new_quoin.transform.localPosition = new Vector3(
            x * ((innerScale.x / 2.0f) - (0.15f / 2.0f) - 0.05f),
            (innerScale.y / 2.0f) + (0.175f / 2.0f),
            z * ((innerScale.z / 2.0f) - (0.15f / 2.0f) - 0.05f)
        );

            //new_quoin.transform.RotateAround(transform.position, Vector3.up, transform.localEulerAngles.y);
            new_quoin.transform.localScale = new Vector3(0.15f, 0.175f, 0.15f);
            new_quoin.transform.SetParent(brickStorage.transform, false);
    }
    
    void MoreBricks(float width, float side)
    {
        int numBricksTall = Mathf.RoundToInt(roofHeight / 0.1f);
        float heightOfBrick = roofHeight / numBricksTall;
        numBricksTall += 1;

        for (int i = 0; i < numBricksTall; i++)
        {
            float yShift = (heightOfBrick / 2.0f) + (heightOfBrick * (i - 1));
            if (yShift < 0.0f) yShift = 0.0f;
            float xShift = Mathf.Pow(yShift / roofHeight, 1.0f / roofCurve);

            float distance = width * (1.0f - xShift);

            if (i == 0) {
                distance = width - 0.2f;
                yShift = (heightOfBrick / -2.0f);
            }

            if (distance > width - 0.2f) distance = width - 0.2f;
            int numBricksWide = Mathf.RoundToInt(distance / 0.15f);
            float widthOfBrick = distance / numBricksWide;

            if (numBricksWide == 0) {
                numBricksWide = 1;
                widthOfBrick = 0.1f;
                distance = 0.1f;
            }

            float[,] noise = new float[1, numBricksWide];
            SetNoise(noise, 1, numBricksWide);

            float startPosition = distance / 2.0f;
            float upTo = 0.0f;

            for (int j = 0; j < numBricksWide; j++)
            {
                Brick newBrick = Instantiate(brick);
                float newWidth = numBricksWide == 1 ? widthOfBrick : Mathf.Abs(widthOfBrick + 0.1f * noise[0, j]);
                float zNoise = 0.025f * (Mathf.PerlinNoise(i / 0.8f, j / 0.8f) - 0.5f);

                newBrick.transform.localPosition = new Vector3(
                    startPosition - upTo - (newWidth / 2.0f),
                    (innerScale.y / 2.0f) + yShift + heightOfBrick,
                    side * ((innerScale.z / 2.0f) - 0.125f) + zNoise
                );

                newBrick.transform.Rotate(new Vector3(0.0f, side * 90.0f, 0.0f), Space.Self);

                float rotationalNoise = Mathf.PerlinNoise(i / 0.87f, j / 0.87f);
                if (rotationalNoise > 0.75f)
                {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 90.0f);
                } 
                else if (rotationalNoise > 0.5f)
                {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 180.0f);
                } 
                else if (rotationalNoise > 0.25f)
                {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 270.0f);
                }

                newBrick.transform.localScale = new Vector3(0.1f, heightOfBrick, newWidth);
                newBrick.transform.SetParent(brickStorage.transform, false);

                upTo += newWidth;
            }
        }
    }

    void InitBeams()
    {

    }

    void InitRoof()
    {
        float amount = innerScale.z / 2.0f;

        if (ridgeLength > 0.2f)
        {
            amount *= ridgeLength;
            PlaceShingles(innerScale.z, innerScale.x / 2.0f, amount, false, 1.0f);
            PlaceShingles(innerScale.z, innerScale.x / 2.0f, amount, false, -1.0f);
            PlaceShingles(innerScale.x, amount, innerScale.x / 2.0f, true, 1.0f);
            PlaceShingles(innerScale.x, amount, innerScale.x / 2.0f, true, -1.0f);
            PlaceTopRidgeShingles(ridgeLength);
        }
        else
        {
            PlaceShinglesSimple(innerScale.z, innerScale.x / 2.0f, false, 1.0f);
            PlaceShinglesSimple(innerScale.z, innerScale.x / 2.0f, false, -1.0f);
            MoreBricks(innerScale.x, 1.0f);
            MoreBricks(innerScale.x, -1.0f);
            MoreQuoins(1.0f, innerScale.y, 1.0f);
            MoreQuoins(1.0f, innerScale.y, -1.0f);
            MoreQuoins(-1.0f, innerScale.y, 1.0f);
            MoreQuoins(-1.0f, innerScale.y, -1.0f);
            PlaceTopRidgeShingles(0.2f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBricks();
        InitRoof();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged) {
            if (transform.localScale.x != 1.0f)
                transform.localScale = new Vector3(1.0f, transform.localScale.y, transform.localScale.z);
            if (transform.localScale.y != 1.0f)
                transform.localScale = new Vector3(transform.localScale.x, 1.0f, transform.localScale.z);
            if (transform.localScale.z != 1.0f)
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1.0f);
        }

        if (scaleUpdated)
        {
            Rebuild();
        }
        else if (updatedLastFrame)
        {
            HandleInteractions();
            HandleInteractions();
        }
    }
}
