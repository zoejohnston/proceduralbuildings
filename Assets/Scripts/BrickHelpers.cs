using UnityEngine;

/// <summary>
/// Some helper methods for building with bricks.
/// </summary>
public static class BrickHelpers
{   
    /// <summary>
    /// Fills the array <c>noise</c> with Perlin noise. The sum of a row of noise is always equal to zero.
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

            for (int j = 1; j < numBricksWide - 1; j++) {
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
        // Determines which wall we are placing bricks on, the one in the positive direction or
        // the one in the negative
        float direction = 1.0f;
        if (rotation > 100.0f) direction = -1.0f;
        // Determines if we are moving along the x axis or the z axis as we place bricks
        bool swap = false;
        if (rotation < 50.0f || (100.0f < rotation && rotation < 200.0f)) swap = true;

        // Get the size of the building part
        Vector3 innerScale = buildingPart.GetScale();

        // Placing numBricksTall * numBricksWide bricks
        for (int i = 0; i < numBricksTall; i++) {
            // Keeping track of how for along this row we are
            float upTo = 0.0f;

            for (int j = 0; j < numBricksWide; j ++) {
                Brick newBrick = Object.Instantiate(buildingPart.brick);

                // Adding randomness into the brick width
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

                // Rotating brick meshes so that there is more variation
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

                // Will be used later to split some bricks in two to add visual interest
                newBrick.splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                newBrick.splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);
                newBrick.shouldntSplit = false;

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
        // Get the size of the building part
        Vector3 innerScale = buildingPart.GetScale();

        float epsilon = 0.000001f;
        float widthOfBrick = buildingPart.defaultQuoinWidth / 2.0f;
        float lenthOfBrick = buildingPart.defaultQuoinWidth;
        height += epsilon;

        // When the roof is curved, the walls need to be a bit taller. The float addedHeight accounts for this.
        float positionOnRoof = 1.0f - (((innerScale.x / 2.0f) - 0.05f) / (innerScale.x / 2.0f));
        float addedHeight = Mathf.Pow(positionOnRoof, buildingPart.roofCurve) * buildingPart.roofHeight;
        height += buildingPart.ridgeLength > 0.2f ? 0.0f : addedHeight;

        int numBricksTall = Mathf.RoundToInt(height / buildingPart.defaultQuoinHeight);
        float heightOfBrick = height / numBricksTall;

        // Placing quoins for this corner
        for (int i = 0; i < numBricksTall; i++) {
            // We switch the longer direction of the quoin back and forth
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

            // Rotating the quoins randomly for more variation
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
    /// <param name="x">Should be -1.0 or 1.0. Indicates with <c>z</c> which corner we are placing a quoin on.</param>
    /// <param name="z">Should be -1.0 or 1.0. Indicates with <c>x</c> which corner we are placing a quoin on.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void MoreQuoins(float x, float z, BuildingPart buildingPart)
    {
        // Get the size of the building part
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

        // Rotating the quoins randomly for more variation
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
    /// <param name="width">The width of the building.</param>
    /// <param name="side">Indicates which side of the building we are placing extra bricks on.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void MoreBricks(float width, float side, BuildingPart buildingPart)
    {
        // Get the size of the building part
        Vector3 innerScale = buildingPart.GetScale();
        int numBricksTall = Mathf.RoundToInt(buildingPart.roofHeight / buildingPart.defaultBrickHeight);
        float heightOfBrick = buildingPart.roofHeight / numBricksTall;

        // If the building part is not wood framed, the walls should be a bit taller
        if (!buildingPart.woodFramed) numBricksTall += 1;

        for (int i = 0; i < numBricksTall; i++) {
            // Inverts the roof curve equation
            float yShift = (heightOfBrick / 2.0f) + (heightOfBrick * (i - 1));
            if (buildingPart.woodFramed) yShift = heightOfBrick * i;
            if (yShift < 0.0f) yShift = 0.0f;
            float xShift = Mathf.Pow(yShift / buildingPart.roofHeight, 1.0f / buildingPart.roofCurve);

            // This is the distance between the two sides of the roof at this row of bricks
            float distance = width * (1.0f - xShift);

            // Fixing the first row
            if (i == 0) {
                if (buildingPart.woodFramed) continue;
                distance = width - 0.2f;
                yShift = heightOfBrick / -2.0f;
            }

            // If the building part is wood framed, we want our bricks a bit lower
            if (buildingPart.woodFramed) yShift = (heightOfBrick * (i - 1)) - (heightOfBrick / 2.0f);

            // The distance should never exceed the normal length of the wall
            if (distance > width - 0.2f) distance = width - 0.2f;
            int numBricksWide = Mathf.RoundToInt(distance / buildingPart.defaultBrickWidth);
            float widthOfBrick = distance / numBricksWide;

            // If not wood framed, make sure there is a nice single brick at the top of the wall
            if (numBricksWide == 0) {
                if (buildingPart.woodFramed) continue;
                numBricksWide = 1;
                widthOfBrick = buildingPart.defaultBrickWidth * (2.0f / 3.0f);
                distance = widthOfBrick;
            }

            // Generating noise 
            float[,] noise = new float[1, numBricksWide];
            SetNoise(noise, 1, numBricksWide, i);

            float startPosition = distance / 2.0f;
            float upTo = 0.0f;

            // Adding a row of bricks
            for (int j = 0; j < numBricksWide; j++) {
                Brick newBrick = Object.Instantiate(buildingPart.brick);

                // Adding randomness into the brick widths
                float newWidth = numBricksWide == 1 ? widthOfBrick : Mathf.Abs(widthOfBrick + 0.1f * noise[0, j]);
                float zNoise = 0.025f * (Mathf.PerlinNoise(i / 0.8f, j / 0.8f) - 0.5f);

                newBrick.transform.localPosition = new Vector3(
                    startPosition - upTo - (newWidth / 2.0f),
                    (innerScale.y / 2.0f) + yShift + heightOfBrick,
                    side * ((innerScale.z / 2.0f) - 0.125f) + zNoise
                );

                newBrick.transform.Rotate(new Vector3(0.0f, side * 90.0f, 0.0f), Space.Self);

                // Rotating brick meshes so that there is more variation
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

                // Will be used later to split some bricks in two to add visual interest
                newBrick.splitNoise = Mathf.PerlinNoise(i / 0.8f, j / 0.8f);
                newBrick.splitLocation = 0.7f * (Mathf.PerlinNoise(i / 0.3f, j / 0.3f) - 0.5f);
                newBrick.shouldntSplit = false;

                upTo += newWidth;
            }
        }
    }

    /// <summary>
    /// Builds a support for a small overhang
    /// </summary>
    /// <param name="start">The position of the start of the support.</param>
    /// <param name="end">The position of the end of the support.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void BuildSupport(Vector3 start, Vector3 end, BuildingPart buildingPart)
    {
        float lengthAvailable = Vector3.Distance(start, end);
        float firstBrickDefault = 0.05f;

        // Adds in the first support brick
        Brick newBrick = Object.Instantiate(buildingPart.brick);
        newBrick.transform.SetParent(buildingPart.brickStorage.transform, false);

        // If we have more length available than the max length of the first brick, add another brick
        if (lengthAvailable > firstBrickDefault) {
            newBrick.transform.position = end + (0.5f * firstBrickDefault * Vector3.Normalize(start - end)) + (0.05f * Vector3.down);
            newBrick.transform.localScale = new Vector3(0.05f, 0.1f, firstBrickDefault);

            // Determines how long the seconf brick should be
            float lengthOfBrick = lengthAvailable > firstBrickDefault + 0.1f ? 0.1f : lengthAvailable - firstBrickDefault;

            Brick newBrick2 = Object.Instantiate(buildingPart.brick);
            newBrick2.transform.SetParent(buildingPart.brickStorage.transform, false);
            newBrick2.transform.position = end + ((firstBrickDefault + 0.5f * lengthOfBrick) * Vector3.Normalize(start - end)) + (0.025f * Vector3.down);
            newBrick2.transform.localScale = new Vector3(0.05f, 0.05f, lengthOfBrick);
            buildingPart.interBuildingPartObjects.Add(newBrick2.gameObject);

        // If not, cap the length of the first brick and move on
        } else {
            newBrick.transform.position = end + (0.5f * lengthAvailable * Vector3.Normalize(start - end)) + (0.05f * Vector3.down);
            newBrick.transform.localScale = new Vector3(0.05f, 0.1f, lengthAvailable);
        }

        buildingPart.interBuildingPartObjects.Add(newBrick.gameObject);
    }

    /// <summary>
    /// Used by BuildSignificantSupport to build one brick level of a column.
    /// </summary>
    /// <param name="center">The center of the column at this level.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    /// <param name="rotate">Indicates that the brick placements should be flipped for this level.</param>
    /// <param name="widthMultiplier">The width of the column at this level.</param>
    /// <param name="heightOfBrick">The height of the bricks.</param>
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
    /// <param name="start">The start location for the support column.</param>
    /// <param name="i">With <c>j</c>, indicates which column we are looking at. Should be 1 or -1.</param>
    /// <param name="j">With <c>i</c>, indicates which column we are looking at. Should be 1 or -1.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void BuildSignificantSupport(Vector3 start, int i, int j, BuildingPart buildingPart)
    {
        RaycastHit hit;
        Vector3 endPoint;
        LayerMask mask = ~LayerMask.GetMask("Windows", "Default");

        // Determines where the column will end
        if (Physics.Raycast(start, Vector3.down, out hit, start.y, mask)) {
            endPoint = hit.point + (0.05f * Vector3.down);
        } else {
            endPoint = new Vector3(start.x, 0.0f, start.z);
        }

        float supportLength = Vector3.Distance(start, endPoint);
        int numBricksTall = Mathf.RoundToInt(supportLength / 0.1f);
        float heightOfBrick = supportLength / numBricksTall;

        // Builds the support column
        for (int k = 0; k < numBricksTall; k++) {
            // Uses parabola to determine the width of the column at each height
            float width = 1.0f;
            if (numBricksTall > 2) width = ((2.0f * k / (numBricksTall - 2)) - 1) * ((2.0f * k / (numBricksTall - 2)) - 1);
            if (k == 0) { width = 1.1f; }
            if (width < 0.6f) { width = 0.6f; }
            else if (width > 1.0f) { width = 1.0f; }
            else { width = 0.8f; }

            // Builds a layer of the support column
            Vector3 center = start + (((k * heightOfBrick) + (heightOfBrick / 2.0f)) * Vector3.down);
            center += buildingPart.transform.TransformDirection(new Vector3(-1 * i * 0.1f, 0.0f, -1 * j * 0.1f));
            BuildColumnLevel(center, buildingPart, k % 2 == 0, width, heightOfBrick);
        }
    }
}
