using UnityEngine;

public static class BrickHelpers
{
    /// <summary>
    /// Builds a single brick wall. Meant to be used four times in order to build a building part.
    /// </summary>
    /// <param name="numBricksTall">The number of bricks tall that the wall should be.</param>
    /// <param name="numBricksWide">The number of bricks wide that the wall should be.</param>
    /// <param name="heightOfBrick">The height of each brick.</param>
    /// <param name="widthOfBrick">The default width of each brick.</param>
    /// <param name="noise">Used to vary the width of each brick. This parameter should be generated using <c>SetNoise</c> so that the width of the wall stays the same.</param>
    /// <param name="rotation">The rotation needed to rotate the wall such that its normal points outwards.</param>
    /// <param name="shrink">How far inset from the roof the wall should be.</param>
    public static void BuildBrickWall(
        int numBricksTall, int numBricksWide, float heightOfBrick, float widthOfBrick, 
        float[,] noise, float rotation, float shrink, BuildingPart buildingPart
    ) {
        float direction = 1.0f;
        if (rotation > 100.0f) direction = -1.0f;
        bool swap = false;
        if (rotation < 50.0f || (100.0f < rotation && rotation < 200.0f)) swap = true;

        Vector3 innerScale = buildingPart.GetScale();

        for (int i = 0; i < numBricksTall; i++)
        {
            float upTo = 0.0f;
            for (int j = 0; j < numBricksWide; j ++)
            {
                Brick newBrick = Object.Instantiate(buildingPart.brick);
                float newWidth = Mathf.Abs(widthOfBrick + 0.1f * noise[i, j]);
                float zNoise = 0.025f * (Mathf.PerlinNoise(i / 0.8f, j / 0.8f) - 0.5f);
                
                if (swap) {
                    newBrick.transform.localPosition = new Vector3(
                        -direction * ((innerScale.x / 2.0f) - (0.25f / 2.0f)) + zNoise,
                        -(innerScale.y / 2.0f) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        ((innerScale.z / 2.0f) - shrink) - upTo - (newWidth / 2.0f)
                    );
                } else {
                    newBrick.transform.localPosition = new Vector3(
                        ((innerScale.x / 2.0f) - shrink) - upTo - (newWidth / 2.0f),
                        -(innerScale.y / 2.0f) + (heightOfBrick / 2.0f) + (heightOfBrick * i),
                        direction * ((innerScale.z / 2.0f) - (0.25f / 2.0f)) + zNoise
                    );
                }
                
                newBrick.name = "Brick[" + i + "," + j + "]" + rotation;

                float rotationalNoise = Mathf.PerlinNoise(i / 0.87f, j / 0.87f);
                if (rotationalNoise > 0.75f) {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 90.0f);
                } else if (rotationalNoise > 0.5f) {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 180.0f);
                } else if (rotationalNoise > 0.25f) {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 270.0f);
                }

                newBrick.transform.localEulerAngles = new Vector3(0.0f, rotation, 0.0f);
                newBrick.transform.localScale = new Vector3(0.1f, heightOfBrick, newWidth);
                newBrick.transform.SetParent(buildingPart.brickStorage.transform, false);

                newBrick.splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                newBrick.splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);

                upTo += newWidth;
            }
        }
    }

    /// <summary>
    /// Initializes corner bricks.
    /// </summary>
    /// <param name="xFlip">Either 1.0f or -1.0f. Indicates on which side of the building we are placing corner bricks, in the x-direction.</param>
    /// <param name="height">The height of the building.</param>
    /// <param name="zFlip">Either 1.0f or -1.0f. Indicates on which side of the building we are placing corner bricks, in the z-direction.</param>
    public static void InitQuoins(float xFlip, float height, float zFlip, BuildingPart buildingPart) {
        Vector3 innerScale = buildingPart.GetScale();

        float epsilon = 0.000001f;
        float widthOfBrick = 0.1f;
        float lenthOfBrick = 0.2f;
        height += epsilon;

        float positionOnRoof = 1.0f - (((innerScale.x / 2.0f) - 0.05f) / (innerScale.x / 2.0f));
        float addedHeight = Mathf.Pow(positionOnRoof, buildingPart.roofCurve) * buildingPart.roofHeight;
        height += buildingPart.ridgeLength > 0.2f ? 0.0f : addedHeight;

        int numBricksTall = Mathf.RoundToInt(height / 0.15f);
        float heightOfBrick = height / numBricksTall;

        for (int i = 0; i < numBricksTall; i++)
        {
            float temp = widthOfBrick;
            widthOfBrick = lenthOfBrick;
            lenthOfBrick = temp;

            Quoin newQuoin = Object.Instantiate(buildingPart.quoin);
            float sizeNoise = widthOfBrick * (Mathf.Clamp(Mathf.PerlinNoise(i / 0.32f, (xFlip * innerScale.x) + (zFlip * innerScale.z)), 0.3f, 0.7f) - 0.3f);
            float placementNoise = 0.1f * widthOfBrick * (Mathf.Clamp(Mathf.PerlinNoise(i / 0.78f, 0.1f * (xFlip * innerScale.x) + (zFlip * innerScale.z)), 0.3f, 0.7f) - 0.3f);
            float thisWidth = widthOfBrick - (i % 2 == 0 ? sizeNoise : 0.0f);
            float thisLength = lenthOfBrick - (i % 2 == 1 ? sizeNoise : 0.0f);

            newQuoin.transform.localPosition = new Vector3(
                xFlip * ((innerScale.x / 2.0f) - ((thisWidth + placementNoise) / 2.0f) - 0.05f),
                -(innerScale.y / 2.0f) + (heightOfBrick / 2.0f) + (heightOfBrick * i) - (epsilon / 2.0f),
                zFlip * ((innerScale.z / 2.0f) - ((thisLength + placementNoise) / 2.0f) - 0.05f)
            );

            newQuoin.transform.localScale = new Vector3(thisWidth, heightOfBrick, thisLength);
            newQuoin.transform.SetParent(buildingPart.brickStorage.transform, false);

            float rotationalNoise = Mathf.PerlinNoise(i / 0.87f, newQuoin.transform.position.x + newQuoin.transform.position.z);
            if (rotationalNoise > 0.75f) {
                newQuoin.transform.GetChild(1).RotateAround(newQuoin.transform.position, Vector3.up, 90.0f);
            } else if (rotationalNoise > 0.5f) {
                newQuoin.transform.GetChild(1).RotateAround(newQuoin.transform.position, Vector3.up, 180.0f);
            } else if (rotationalNoise > 0.25f) {
                newQuoin.transform.GetChild(1).RotateAround(newQuoin.transform.position, Vector3.up, 270.0f);
            }
        }
    }
}
