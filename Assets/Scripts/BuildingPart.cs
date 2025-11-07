using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BuildingPart : MonoBehaviour
{
    public Brick brick;
    public Quoin quoin;
    public Shingle shingle;
    public GameObject storage;

    [Range(0.5f, 1.75f)]
    public float roofCurve = 1.0f;

    [Range(0.1f, 2.0f)]
    public float roofHeight = 2.0f;

    [Range(0.2f, 1.0f)]
    public float ridgeLength = 0.5f;

    //private float defaultBrickHeight = 0.25f;
    //private float defaultBrickLength = 0.0125f;
    //private float defaultQuoinHeight = 0.25f;
    //private float defaultQuoinLength = 0.05f;

    void InitQuoins(float x, float y, float z)
    {
        float epsilon = 0.000001f;
        y += epsilon;

        int numBricksTall = Mathf.RoundToInt(y / 0.5f);
        float heightOfBrick = y / numBricksTall;

        float widthOfBrick = 0.4f;
        float lenthOfBrick = 0.7f;

        for (int i = 0; i < numBricksTall; i++)
        {
            float temp = widthOfBrick;
            widthOfBrick = lenthOfBrick;
            lenthOfBrick = temp;

            Quoin new_quoin = Instantiate(quoin);

            new_quoin.transform.localPosition = new Vector3(
                transform.position.x + x * ((transform.localScale.x / 2.0f) - (widthOfBrick / 2.0f) - 0.05f),
                (transform.position.y - (transform.localScale.y / 2.0f)) + (heightOfBrick / 2.0f) + (heightOfBrick * i) - (epsilon / 2.0f),
                transform.position.z + z * ((transform.localScale.z / 2.0f) - (lenthOfBrick / 2.0f) - 0.05f)
            );

            new_quoin.transform.RotateAround(transform.position, Vector3.up, transform.localEulerAngles.y);
            new_quoin.transform.localScale = new Vector3(widthOfBrick, heightOfBrick, lenthOfBrick);
            new_quoin.transform.SetParent(storage.transform);
            new_quoin.name = "Quoin[" + x + "," + z + "]" + i;
        }
    }

    void SetNoise(float[,] noise, int numBricksTall, int numBricksWide)
    {
        for (int i = 0; i < numBricksTall; i++)
        {
            float sum = 0.0f;

            noise[i, 0] = Mathf.PerlinNoise(i / 0.7f, 0 / 0.7f) - 0.5f;
            if (i % 2 == 0)
            {
                noise[i, 0] += 0.2f;
            }
            else
            {
                noise[i, 0] -= 0.2f;
            }
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
                float newWidth = Mathf.Abs(widthOfBrick + 0.4f * noise[i, j]);

                if (swap)
                {
                    newBrick.transform.localPosition = new Vector3(
                        transform.position.x - direction * ((transform.localScale.x / 2.0f) - (shrink / 2.0f)),
                        (transform.position.y - (transform.localScale.y / 2.0f)) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        (transform.position.z + (transform.localScale.z / 2.0f) - shrink) - upTo - (newWidth / 2.0f)
                    );
                }
                else
                {
                    newBrick.transform.localPosition = new Vector3(
                        (transform.position.x + (transform.localScale.x / 2.0f) - shrink) - upTo - (newWidth / 2.0f),
                        (transform.position.y - (transform.localScale.y / 2.0f)) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        transform.position.z + direction * ((transform.localScale.z / 2.0f) - (shrink / 2.0f))
                    );
                }
                
                newBrick.name = "Brick[" + i + "," + j + "]" + rotation;

                newBrick.transform.localEulerAngles = new Vector3(0.0f, rotation, 0.0f);
                newBrick.transform.RotateAround(transform.position, Vector3.up, transform.localEulerAngles.y);
                newBrick.transform.localScale = new Vector3(0.1f, heightOfBrick, newWidth);
                newBrick.transform.SetParent(storage.transform);

                float splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                float splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);
                if (splitNoise > 0.65f) newBrick.Split(splitLocation);/**/

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

    void InitBricks()
    {
        float shrink = 0.6f;
        float height = transform.localScale.y;
        InitQuoins(1.0f, height, 1.0f);
        InitQuoins(1.0f, height, -1.0f);
        InitQuoins(-1.0f, height, 1.0f);
        InitQuoins(-1.0f, height, -1.0f);

        // Rest of bricks
        int numBricksTall = Mathf.RoundToInt(height / 0.25f);
        float heightOfBrick = height / numBricksTall;

        float width = transform.localScale.x - shrink;
        int numBricksWide = Mathf.RoundToInt(width / 0.3f);
        float widthOfBrick = width / numBricksWide;

        float[,] noise = new float[numBricksTall, numBricksWide];
        SetNoise(noise, numBricksTall, numBricksWide);

        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 90.0f, shrink / 2.0f);
        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 270.0f, shrink / 2.0f);
        
        width = transform.localScale.z - shrink;
        numBricksWide = Mathf.RoundToInt(width / 0.3f);
        widthOfBrick = width / numBricksWide;

        noise = new float[numBricksTall, numBricksWide];
        SetNoise(noise, numBricksTall, numBricksWide);

        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 0.0f, shrink / 2.0f);
        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 180.0f, shrink / 2.0f);
    }

    void UpdateBricks()
    {
        foreach (Transform childTransform in storage.transform)
        {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent<Quoin>(out Quoin quoin)) quoin.DeletePls();
            if (childObject.TryGetComponent<Brick>(out Brick brick)) brick.DeletePls();
            if (childObject.TryGetComponent<Shingle>(out Shingle shingle)) shingle.DeletePls();
        }

        InitBricks();
        InitRoof();

        Physics.simulationMode = SimulationMode.Script;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.simulationMode = SimulationMode.FixedUpdate;
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
    
    void PlaceShingles(float width, float depth, float otherWidth, bool flip, float side, bool snapped)
    {
        int numShinglesWide = Mathf.RoundToInt(width / 0.1f);
        float widthOfShingle = width / numShinglesWide;

        float arcLength = EstimateArcLength(depth);
        int numShinglesTall = Mathf.RoundToInt(arcLength / 0.1f);
        float lengthOfShingle = arcLength / numShinglesTall;

        float current = Mathf.Epsilon;

        while (current < 1.0f)
        {
            float xShift = current * depth;
            float yShift = Mathf.Pow(current, roofCurve) * roofHeight;

            current += ShingleStep(xShift, yShift, current, lengthOfShingle, depth);

            Vector2 tangent = new Vector2(1.0f, roofHeight * roofCurve * Mathf.Pow(current, roofCurve - 1.0f));
            tangent.Normalize();

            for (int j = 0; j < numShinglesWide; j++)
            {
                if (!snapped)
                {
                    float scaledWidth = 2.0f;
                    if (!flip) scaledWidth /= ridgeLength;
                    if (yShift > Mathf.Pow((j * widthOfShingle) / otherWidth, roofCurve) * roofHeight) continue;
                    if (yShift > Mathf.Pow(scaledWidth - ((j * widthOfShingle) / otherWidth), roofCurve) * roofHeight) continue;
                }
                
                Shingle newShingle = Instantiate(shingle);

                if (flip)
                {
                    newShingle.transform.localPosition = new Vector3(
                            transform.position.x + (width / 2.0f) - (j * widthOfShingle),
                            transform.position.y + (transform.localScale.y / 2.0f) + yShift,
                            transform.position.z + side * ((transform.localScale.z / 2.0f) - xShift)
                        );
                }
                else
                {
                    newShingle.transform.localPosition = new Vector3(
                            transform.position.x + side * ((transform.localScale.x / 2.0f) - xShift),
                            transform.position.y + (transform.localScale.y / 2.0f) + yShift,
                            transform.position.z + (width / 2.0f) - (j * widthOfShingle)
                        );
                }

                float angle = (-1.0f * side * Vector2.Angle(Vector2.up, tangent)) - (side * 8.0f);
                if (!flip) newShingle.transform.localRotation = Quaternion.Euler(angle, 90.0f, 0.0f);
                if (flip) newShingle.transform.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
                newShingle.transform.localScale = new Vector3(widthOfShingle * 0.9f, 0.15f, 0.25f);
                newShingle.transform.SetParent(storage.transform);
            }
        }
    }

    void InitRoof()
    {
        float amount = transform.localScale.z / 2.0f;

        if (ridgeLength > 0.2f)
        {
            amount *= ridgeLength;
            PlaceShingles(transform.localScale.x, amount, transform.localScale.x / 2.0f, true, 1.0f, false);
            PlaceShingles(transform.localScale.x, amount, transform.localScale.x / 2.0f, true, -1.0f, false);
            PlaceShingles(transform.localScale.z, transform.localScale.x / 2.0f, amount, false, 1.0f, false);
            PlaceShingles(transform.localScale.z, transform.localScale.x / 2.0f, amount, false, -1.0f, false);
        }
        else
        {
            PlaceShingles(transform.localScale.z, transform.localScale.x / 2.0f, amount, false, 1.0f, true);
            PlaceShingles(transform.localScale.z, transform.localScale.x / 2.0f, amount, false, -1.0f, true);
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
        if (transform.hasChanged)
        {
            if (transform.localScale.x < 1.5f)
                transform.localScale = new Vector3(1.5f, transform.localScale.y, transform.localScale.z);
            if (transform.localScale.y < 0.5f)
                transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
            if (transform.localScale.z < 1.5f)
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1.5f);

            transform.hasChanged = false;
        }

        if (Selection.Contains(gameObject))
        {
            UpdateBricks();
        }
    }
}
