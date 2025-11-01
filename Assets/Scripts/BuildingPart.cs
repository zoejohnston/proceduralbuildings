using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BuildingPart : MonoBehaviour
{
    public Brick brick;
    public Quoin quoin;
    public GameObject storage;

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
                transform.position.x + x * ((transform.localScale.x / 2.0f) - (widthOfBrick / 2.0f) + 0.15f),
                (transform.position.y - (transform.localScale.y / 2.0f)) + (heightOfBrick / 2.0f) + (heightOfBrick * i) - (epsilon / 2.0f),
                transform.position.z + z * ((transform.localScale.z / 2.0f) - (lenthOfBrick / 2.0f) + 0.15f)
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
    
    void BuildBrickWall(int numBricksTall, int numBricksWide, float heightOfBrick, float widthOfBrick, float[,] noise, float rotation)
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
                        transform.position.x - direction * ((transform.localScale.x / 2.0f) + 0.05f),
                        (transform.position.y - (transform.localScale.y / 2.0f)) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        (transform.position.z + (transform.localScale.z / 2.0f)) - upTo - (newWidth / 2.0f)
                    );
                }
                else
                {
                    newBrick.transform.localPosition = new Vector3(
                        (transform.position.x + (transform.localScale.x / 2.0f)) - upTo - (newWidth / 2.0f),
                        (transform.position.y - (transform.localScale.y / 2.0f)) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        transform.position.z + direction * ((transform.localScale.z / 2.0f) + 0.05f)
                    );
                }

                if (newWidth < 0.0f || heightOfBrick < 0.0f)
                {
                    Debug.Log("Yikes!");
                }
                newBrick.name = "Brick[" + i + "," + j + "]" + rotation;

                newBrick.transform.localEulerAngles = new Vector3(0.0f, rotation, 0.0f);
                newBrick.transform.RotateAround(transform.position, Vector3.up, transform.localEulerAngles.y);
                newBrick.transform.localScale = new Vector3(0.1f, heightOfBrick, newWidth);
                newBrick.transform.SetParent(storage.transform);

                float splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                float splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);
                if (splitNoise > 0.65f) newBrick.Split(splitLocation);

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
        float height = transform.localScale.y;
        InitQuoins(1.0f, height, 1.0f);
        InitQuoins(1.0f, height, -1.0f);
        InitQuoins(-1.0f, height, 1.0f);
        InitQuoins(-1.0f, height, -1.0f);

        // Rest of bricks
        int numBricksTall = Mathf.RoundToInt(height / 0.25f);
        float heightOfBrick = height / numBricksTall;

        int numBricksWide = Mathf.RoundToInt(transform.localScale.x / 0.3f);
        float widthOfBrick = transform.localScale.x / numBricksWide;

        float[,] noise = new float[numBricksTall, numBricksWide];
        SetNoise(noise, numBricksTall, numBricksWide);

        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 90.0f);
        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 270.0f);
        
        numBricksWide = Mathf.RoundToInt(transform.localScale.z / 0.3f);
        widthOfBrick = transform.localScale.z / numBricksWide;

        noise = new float[numBricksTall, numBricksWide];
        SetNoise(noise, numBricksTall, numBricksWide);

        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 0.0f);
        BuildBrickWall(numBricksTall, numBricksWide, heightOfBrick, widthOfBrick, noise, 180.0f);
    }

    void UpdateBricks()
    {
        foreach (Transform childTransform in storage.transform)
        {
            GameObject childObject = childTransform.gameObject;

            if (childObject.TryGetComponent<Quoin>(out Quoin quoin)) quoin.DeletePls(); 
            if (childObject.TryGetComponent<Brick>(out Brick brick)) brick.DeletePls(); 
        }

        InitBricks();

        Physics.simulationMode = SimulationMode.Script;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.simulationMode = SimulationMode.FixedUpdate;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBricks();
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
