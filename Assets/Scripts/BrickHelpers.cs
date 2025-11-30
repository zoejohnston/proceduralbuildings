using UnityEngine;

/// <summary>
/// Some helper methods for building with bricks.
/// </summary>
public static class BrickHelpers
{   
    /// <summary>
    /// Fills the array <c>noise</c> with Perlin noise. The sum of a row of noise is always equal to one.
    /// </summary>
    /// <param name="noise">The array to fill. Should be an <c>m</c> by <c>n</c> array.</param>
    /// <param name="numBricksTall">The first dimension of the array, <c>m</c>.</param>
    /// <param name="numBricksWide">The second dimension of the array, <c>n</c>.</param>
    /// <param name="otherInput">Used to add in extra variability between calls to this method.</param>
    public static void SetNoise(float[,] noise, int numBricksTall, int numBricksWide, int otherInput)
    {
        for (int i = 0; i < numBricksTall; i++)
        {
            float sum = 0.0f;
            noise[i, 0] = Mathf.PerlinNoise((i + otherInput) / 0.7f, 0 / 0.7f) - 0.5f;
            sum += noise[i, 0];

            for (int j = 1; j < numBricksWide - 1; j++)
            {
                noise[i, j] = Mathf.PerlinNoise((i + otherInput) / 0.7f, j / 0.7f) - 0.5f;
                sum += noise[i, j];
            }

            noise[i, numBricksWide - 1] = -1.0f * sum;
        }
    }

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

    /// <summary>
    /// When the roof ridge is as long as possible, we need to extend the wall up to meet the roof. This method handles
    /// the corner bricks for the extended wall.
    /// </summary>
    /// <param name="x">.</param>
    /// <param name="z">.</param>
    /// <param name="buildingPart">.</param>
    public static void MoreQuoins(float x, float z, BuildingPart buildingPart)
    {
        Vector3 innerScale = buildingPart.GetScale();
        Quoin newQuoin = Object.Instantiate(buildingPart.quoin);
        float positionOnRoof = 1.0f - (((innerScale.x / 2.0f) - 0.05f) / (innerScale.x / 2.0f));
        float addedHeight = Mathf.Pow(positionOnRoof, buildingPart.roofCurve) * buildingPart.roofHeight;

        newQuoin.transform.localPosition = new Vector3(
            x * ((innerScale.x / 2.0f) - (0.15f / 2.0f) - 0.05f),
            (innerScale.y / 2.0f) + (0.15f / 2.0f) + addedHeight,
            z * ((innerScale.z / 2.0f) - (0.125f / 2.0f) - 0.05f)
        );

        newQuoin.transform.localScale = new Vector3(0.15f, 0.15f, 0.125f);
        newQuoin.transform.SetParent(buildingPart.brickStorage.transform, false);

        float rotationalNoise = Mathf.PerlinNoise(newQuoin.transform.position.x, newQuoin.transform.position.z);
        if (rotationalNoise > 0.75f) {
            newQuoin.transform.GetChild(1).RotateAround(newQuoin.transform.position, Vector3.up, 90.0f);
        } else if (rotationalNoise > 0.5f) {
            newQuoin.transform.GetChild(1).RotateAround(newQuoin.transform.position, Vector3.up, 180.0f);
        } else if (rotationalNoise > 0.25f) {
            newQuoin.transform.GetChild(1).RotateAround(newQuoin.transform.position, Vector3.up, 270.0f);
        }
    }

    /// <summary>
    /// When the roof ridge is as long as possible, we need to extend the wall up to meet the roof. This method handles
    /// the bricks for the extended wall.
    /// </summary>
    /// <param name="width">.</param>
    /// <param name="side">.</param>
    /// <param name="buildingPart">.</param>
    public static void MoreBricks(float width, float side, BuildingPart buildingPart)
    {
        Vector3 innerScale = buildingPart.GetScale();
        int numBricksTall = Mathf.RoundToInt(buildingPart.roofHeight / 0.1f);
        float heightOfBrick = buildingPart.roofHeight / numBricksTall;
        if (!buildingPart.woodFramed) numBricksTall += 1;

        for (int i = 0; i < numBricksTall; i++) {
            float yShift = (heightOfBrick / 2.0f) + (heightOfBrick * (i - 1));
            if (buildingPart.woodFramed) yShift = heightOfBrick * i;
            if (yShift < 0.0f) yShift = 0.0f;
            float xShift = Mathf.Pow(yShift / buildingPart.roofHeight, 1.0f / buildingPart.roofCurve);

            float distance = width * (1.0f - xShift);

            if (i == 0) {
                if (buildingPart.woodFramed) continue;
                distance = width - 0.2f;
                yShift = heightOfBrick / -2.0f;
            }

            if (buildingPart.woodFramed) yShift = (heightOfBrick * (i - 1)) - (heightOfBrick / 2.0f);

            if (distance > width - 0.2f) distance = width - 0.2f;
            int numBricksWide = Mathf.RoundToInt(distance / 0.15f);
            float widthOfBrick = distance / numBricksWide;

            if (numBricksWide == 0) {
                if (buildingPart.woodFramed) continue;
                numBricksWide = 1;
                widthOfBrick = 0.1f;
                distance = 0.1f;
            }

            float[,] noise = new float[1, numBricksWide];
            SetNoise(noise, 1, numBricksWide, i);

            float startPosition = distance / 2.0f;
            float upTo = 0.0f;

            for (int j = 0; j < numBricksWide; j++)
            {
                Brick newBrick = Object.Instantiate(buildingPart.brick);
                float newWidth = numBricksWide == 1 ? widthOfBrick : Mathf.Abs(widthOfBrick + 0.1f * noise[0, j]);
                float zNoise = 0.025f * (Mathf.PerlinNoise(i / 0.8f, j / 0.8f) - 0.5f);

                newBrick.transform.localPosition = new Vector3(
                    startPosition - upTo - (newWidth / 2.0f),
                    (innerScale.y / 2.0f) + yShift + heightOfBrick,
                    side * ((innerScale.z / 2.0f) - 0.125f) + zNoise
                );

                newBrick.transform.Rotate(new Vector3(0.0f, side * 90.0f, 0.0f), Space.Self);

                float rotationalNoise = Mathf.PerlinNoise(i / 0.87f, j / 0.87f);
                if (rotationalNoise > 0.75f) {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 90.0f);
                } else if (rotationalNoise > 0.5f) {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 180.0f);
                } else if (rotationalNoise > 0.25f) {
                    newBrick.transform.GetChild(1).RotateAround(newBrick.transform.position, Vector3.up, 270.0f);
                }

                newBrick.transform.localScale = new Vector3(0.1f, heightOfBrick, newWidth);
                newBrick.transform.SetParent(buildingPart.brickStorage.transform, false);

                newBrick.splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                newBrick.splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);

                upTo += newWidth;
            }
        }
    }

    /// <summary>
    /// Builds a support for a small overhang
    /// </summary>
    public static void BuildSupport(Vector3 start, Vector3 end, BuildingPart buildingPart)
    {
        float lengthAvailable = Vector3.Distance(start, end);
        float firstBrickDefault = 0.05f;

        Brick newBrick = Object.Instantiate(buildingPart.brick);
        newBrick.transform.SetParent(buildingPart.brickStorage.transform, false);

        if (lengthAvailable > firstBrickDefault) {
            newBrick.transform.position = end + (0.5f * firstBrickDefault * Vector3.Normalize(start - end)) + (0.05f * Vector3.down);
            newBrick.transform.localScale = new Vector3(0.05f, 0.1f, firstBrickDefault);

            float lengthOfBrick = lengthAvailable > firstBrickDefault + 0.1f ? 0.1f : lengthAvailable - firstBrickDefault;

            Brick newBrick2 = Object.Instantiate(buildingPart.brick);
            newBrick2.transform.SetParent(buildingPart.brickStorage.transform, false);
            newBrick2.transform.position = end + ((firstBrickDefault + 0.5f * lengthOfBrick) * Vector3.Normalize(start - end)) + (0.025f * Vector3.down);
            newBrick2.transform.localScale = new Vector3(0.05f, 0.05f, lengthOfBrick);
            buildingPart.interBuildingPartObjects.Add(newBrick2.gameObject);
        } else {
            newBrick.transform.position = end + (0.5f * lengthAvailable * Vector3.Normalize(start - end)) + (0.05f * Vector3.down);
            newBrick.transform.localScale = new Vector3(0.05f, 0.1f, lengthAvailable);
        }

        buildingPart.interBuildingPartObjects.Add(newBrick.gameObject);
    }

    /// <summary>
    /// Used by BuildSignificantSupport to build one brick level of a column.
    /// </summary>
    private static void BuildColumnLevel(Vector3 center, BuildingPart buildingPart, bool rotate, float widthMultiplier, float heightOfBrick)
    {
        float totalWidth = widthMultiplier * 0.2f;
        float brickDepth = 0.05f;
        float halfBrickDepth = brickDepth / 2.0f;
        float flip = rotate ? -1 : 1;

        for (int i = 0; i < 4; i++) {
            Brick newBrick = Object.Instantiate(buildingPart.brick);
            newBrick.transform.SetParent(buildingPart.brickStorage.transform, false);
            newBrick.transform.position = center;
            newBrick.transform.Translate(new Vector3(flip * halfBrickDepth, 0.0f, (totalWidth / 2.0f) - halfBrickDepth), Space.Self);
            newBrick.transform.localScale = new Vector3(totalWidth - brickDepth, heightOfBrick, brickDepth);
            newBrick.transform.RotateAround(center, Vector3.up, i * 90.0f);
            buildingPart.interBuildingPartObjects.Add(newBrick.gameObject);
        }
    }

    /// <summary>
    /// Builds a support for a large overhang
    /// </summary>
    public static void BuildSignificantSupport(Vector3 start, int i, int j, BuildingPart buildingPart)
    {
        RaycastHit hit;
        Vector3 endPoint;
        LayerMask mask = ~LayerMask.GetMask("Windows", "Default");

        if (Physics.Raycast(start, Vector3.down, out hit, start.y, mask)) {
            endPoint = hit.point + (0.05f * Vector3.down);
        } else {
            endPoint = new Vector3(start.x, 0.0f, start.z);
        }

        float supportLength = Vector3.Distance(start, endPoint);
        int numBricksTall = Mathf.RoundToInt(supportLength / 0.1f);
        float heightOfBrick = supportLength / numBricksTall;

        for (int k = 0; k < numBricksTall; k++) {
            float width = 1.0f;
            if (numBricksTall > 2) width = ((2.0f * k / (numBricksTall - 2)) - 1) * ((2.0f * k / (numBricksTall - 2)) - 1);
            if (k == 0) { width = 1.1f; }
            if (width < 0.6f) { width = 0.6f; }
            else if (width > 1.0f) { width = 1.0f; }
            else { width = 0.8f; }

            Vector3 center = start + (((k * heightOfBrick) + (heightOfBrick / 2.0f)) * Vector3.down);
            center += buildingPart.transform.TransformDirection(new Vector3(-1 * i * 0.1f, 0.0f, -1 * j * 0.1f));
            BuildColumnLevel(center, buildingPart, k % 2 == 0, width, heightOfBrick);
        }
    }
}
