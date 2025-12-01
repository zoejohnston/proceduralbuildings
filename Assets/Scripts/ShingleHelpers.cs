using UnityEngine;

/// <summary>
/// Some helper methods for building with shingles.
/// </summary>
public static class ShingleHelpers
{   
    /// <summary>
    /// Should only be called when ridgeLength is greater than <c>0.2</c>. Places one side's worth of shingles on a 
    /// roof that is sloped on all sides.
    /// </summary>
    /// <param name="width">The width of the roof.</param>
    /// <param name="depth">The depth of the roof.</param>
    /// <param name="otherWidth">The length of the ridge.</param>
    /// <param name="flip">Indicates if we are building the part of the roof parallel to the ridge or orthogonal to it.</param>
    /// <param name="side">Indicates which side of the roof this function should build.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void PlaceShingles(float width, float depth, float otherWidth, bool flip, float side, BuildingPart buildingPart)
    {
        float shingleSize = 0.1f;
        Vector3 innerScale = buildingPart.GetScale();

        // Determine the number of shingles from the base of the roof to the top
        float arcLength = EstimateArcLength(depth, buildingPart);
        int numShinglesTall = Mathf.RoundToInt(arcLength / shingleSize);
        float lengthOfShingle = arcLength / numShinglesTall;

        // Establishing some starting values
        float current = Mathf.Epsilon;

        float xShift = current * depth;
        float yShift = Mathf.Pow(current, buildingPart.roofCurve) * buildingPart.roofHeight;
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

        // Going from the bottom of the roof to the top
        while (current <= 1.0f + Mathf.Epsilon) {
            // Stepping up by one shingle
            float step = ShingleStep(xShift, yShift, current, lengthOfShingle, depth, buildingPart);
            current += step;

            // Our position on the roof
            xShift = current * depth;
            yShift = Mathf.Pow(current, buildingPart.roofCurve) * buildingPart.roofHeight;

            // We only want to place ridge shingles for two out of the four sections of the roof, so that ther is 
            // no overlap.
            if (!flip) {
                rightRidgeShingle = PlaceRidgeShingles(xShift, yShift, current, otherWidth, width, side, 1.0f, rightRidgeShingle, count, buildingPart);
                leftRidgeShingle = PlaceRidgeShingles(xShift, yShift, current, otherWidth, width, side, -1.0f, leftRidgeShingle, count, buildingPart);
                count++;
            }

            // How many shingles are needed from one side of the roof to the other at this height
            float distance = width - (2.0f * (current - step) * otherWidth);
            int numShinglesWide = Mathf.RoundToInt(distance / shingleSize);
            float widthOfShingle = distance / numShinglesWide;

            // Where to start placing shingles
            float startPosition = ((current - step) * otherWidth) - (width / 2.0f);

            // Getting the angle of the shingle
            Vector2 shifts = new Vector2(xShift, yShift);
            Vector2 shiftDiff = shifts - previous;
            float angle = side * Vector2.SignedAngle(Vector2.up, shiftDiff);
            float tilt = side * 15.0f;
            if (!flip) angle = -angle;
            if (flip) tilt = -tilt;

            // Place the shingles
            for (int j = 0; j < numShinglesWide; j++) {
                InstantiateShingle(
                    j, widthOfShingle, lengthOfShingle, numShinglesWide, xShift, yShift, 
                    startPosition, side, angle, tilt, flip, buildingPart
                );
            }

            previous = shifts;
        }
    }

    /// <summary>
    /// Should only be called when ridgeLength is equal to <c>0.2</c>. Places shingles on an A-frame roof.
    /// </summary>
    /// <param name="width">The width of the roof.</param>
    /// <param name="depth">The depth of the roof.</param>
    /// <param name="side">Indicates which side of the roof this function should build.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void PlaceShinglesSimple(float width, float depth, float side, BuildingPart buildingPart)
    {
        float shingleSize = 0.1f;
        Vector3 innerScale = buildingPart.GetScale();

        // Determine the number of shingles from the base of the roof to the top
        float arcLength = EstimateArcLength(depth, buildingPart);
        int numShinglesTall = Mathf.RoundToInt(arcLength / shingleSize);
        float lengthOfShingle = arcLength / numShinglesTall;

        // Establishing some starting values
        float current = Mathf.Epsilon;

        float xShift = current * depth;
        float yShift = Mathf.Pow(current, buildingPart.roofCurve) * buildingPart.roofHeight;
        Vector2 previous = new Vector2(xShift, yShift);

        Vector3 rightRidgeShingle = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            (width / 2.0f) - (buildingPart.woodFramed ? 0.0f : 0.2f)
        );
        Vector3 leftRidgeShingle = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            -(width / 2.0f) + (buildingPart.woodFramed ? 0.0f : 0.2f)
        );

        // Going from the bottom of the roof to the top
        while (current <= 1.0f + Mathf.Epsilon) {
            // Stepping up by one shingle
            float step = ShingleStep(xShift, yShift, current, lengthOfShingle, depth, buildingPart);
            current += step;

            // Our position on the roof
            xShift = current * depth;
            yShift = Mathf.Pow(current, buildingPart.roofCurve) * buildingPart.roofHeight;

            // Place ridge shingles along the edges of the roof
            rightRidgeShingle = PlaceSimpleRidgeShingles(xShift, yShift, width - (buildingPart.woodFramed ? 0.0f : 0.40f), side, 1.0f, rightRidgeShingle, buildingPart);
            leftRidgeShingle = PlaceSimpleRidgeShingles(xShift, yShift, width - (buildingPart.woodFramed ? 0.0f : 0.40f), side, -1.0f, leftRidgeShingle, buildingPart);

            // How many shingles are needed from one side of the roof to the other at this height
            float distance = width - (buildingPart.woodFramed ? -0.15f : 0.15f);
            int numShinglesWide = Mathf.RoundToInt(distance / shingleSize);
            float widthOfShingle = distance / numShinglesWide;

            // Where to start placing shingles
            float startPosition = -distance / 2.0f;

            // Getting the angle of the shingle
            Vector2 shifts = new Vector2(xShift, yShift);
            Vector2 shiftDiff = shifts - previous;
            float angle = -side * Vector2.SignedAngle(Vector2.up, shiftDiff);
            float tilt = side * 15.0f;

            // Place the shingles
            for (int j = 1; j < numShinglesWide - 1; j++) {
                InstantiateShingle(j, widthOfShingle, lengthOfShingle, numShinglesWide, xShift, yShift, startPosition, side, angle, tilt, false, buildingPart);
            }

            previous = shifts;
        }
    }

    /// <summary>
    /// Places a top ridge of shingles along the top of the roof.
    /// </summary>
    /// <param name="length">The length of the ridge.</param>
    /// <param name="buildingPart">The associated BuildingPart.</param>
    public static void PlaceTopRidgeShingles(float length, BuildingPart buildingPart)
    {
        // Gets the size of the building part and determines how many shingles will be needed
        Vector3 innerScale = buildingPart.GetScale();
        float width = ((1.0f - length) * innerScale.z) + 0.09f;
        if (buildingPart.ridgeLength <= 0.2f && !buildingPart.woodFramed) width -= 0.1f;
        int numShinglesWide = Mathf.RoundToInt(width / 0.12f);
        float widthOfShingle = width / numShinglesWide;

        // If no shingles are needed, the roof all meets at one point at the top,
        // we still want one ridge shingle in this case though!
        if (numShinglesWide == 0) {
            numShinglesWide = 1;
            widthOfShingle = 0.09f;
        }

        // Place the shingles
        for (int i = 0; i < numShinglesWide; i++) {
            // if the building part isn't wood framed and is A-frame, we don't need the first and last shingles
            if ((i == 0 || i == numShinglesWide - 1) && buildingPart.ridgeLength <= 0.2f && !buildingPart.woodFramed) continue;

            RidgeShingle newShingle = Object.Instantiate(buildingPart.ridgeShingle);
            float zScale = 0.35f;
            float yScale = 0.01f;
            float zTrans = 0.01f;
            float tilt = 10.0f;

            // Use a mesh without an angle for all shingles except those on the ends
            if (i > 0 && i < numShinglesWide - 1) {
                newShingle.UseFlatMesh();
                zScale = 0.2f;
                yScale = 0.0f;
                zTrans = 0.04f;
                tilt = 0.0f;
            }

            newShingle.transform.localPosition = new Vector3(
                0.0f,
                (innerScale.y / 2.0f) + buildingPart.roofHeight - 0.02f,
                (width / 2.0f) - (i * widthOfShingle)
            );

            // Flip the last ridge shingle
            float angle = i == numShinglesWide - 1 ? 180.0f : 0.0f;
            float flip = i == numShinglesWide - 1 ? -1.0f : 1.0f;

            newShingle.transform.Translate(new Vector3(0.0f, 0.0f, widthOfShingle / -2.0f));
            newShingle.transform.Rotate(flip * tilt - 90.0f, 0.0f, angle, Space.Self);
            newShingle.transform.Translate(new Vector3(0.0f, 0.0f, zTrans - 0.02f), Space.Self);
            newShingle.transform.localScale = new Vector3(0.14f, yScale+widthOfShingle, zScale);
            newShingle.transform.SetParent(buildingPart.shingleStorage.transform, false);
        }
    }

    /// <summary>
    /// Estimates the arc length of roof.
    /// </summary>
    private static float EstimateArcLength(float xDist, BuildingPart buildingPart)
    {
        int numSteps = 20;
        float arcLength = 0.0f;
        Vector2 previousPoint = Vector2.zero;

        for (int i = 1; i <= numSteps; i++) {
            float x = i / (float)numSteps;
            Vector2 currentPoint = new Vector2(x * xDist, Mathf.Pow(x, buildingPart.roofCurve) * buildingPart.roofHeight);
            arcLength += Vector2.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return arcLength;
    }

    /// <summary>
    /// Finds the step size needed to move along the arc length by lengthOfShingle.
    /// </summary>
    private static float ShingleStep(float xShift, float yShift, float current, float lengthOfShingle, float depth, BuildingPart buildingPart)
    {
        float step = 0.0f;
        Vector2 currentPoint = new Vector2(xShift, yShift);
        Vector2 nextPoint = new Vector2((current + step) * depth, Mathf.Pow(current + step, buildingPart.roofCurve) * buildingPart.roofHeight);

        while (Vector2.Distance(currentPoint, nextPoint) < lengthOfShingle) {
            step += 0.001f;
            nextPoint.x = (current + step) * depth;
            nextPoint.y = Mathf.Pow(current + step, buildingPart.roofCurve) * buildingPart.roofHeight;
        }

        return step;
    }

    /// <summary>
    /// Called by PlaceShingles. Places shingles along the edges of a roof that slopes on all sides.
    /// </summary>
    private static Vector3 PlaceRidgeShingles(float xShift, float yShift, float current, float otherWidth, float width, float side, 
        float which, Vector3 previous, int count, BuildingPart buildingPart)
    {
        RidgeShingle newShingle = Object.Instantiate(buildingPart.ridgeShingle);
        Vector3 innerScale = buildingPart.GetScale();

        // Places the shingle
        newShingle.transform.position = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            which * ((width / 2.0f) - (current * otherWidth))
        );

        // Orients the shingle
        float multiplier = count > 0 ? 1.2f : 0.8f;
        float zScale = count > 0 ? 0.25f : 0.14f;
        float extraTranslate = count > 0 ? 0.04f : 0.06f;
        float lengthOfShingle = multiplier * Vector3.Distance(previous, newShingle.transform.position);
        newShingle.transform.LookAt(previous, Vector3.up);

        Vector3 retVal = newShingle.transform.position;

        newShingle.transform.Translate(new Vector3(0.0f, 0.0f, (lengthOfShingle / 2.0f) + extraTranslate));
        newShingle.transform.Rotate(-90.0f - (5.5f * buildingPart.roofCurve), 0.0f, 0.0f, Space.Self);
        newShingle.transform.Translate(new Vector3(0.0f, 0.0f, 0.01f), Space.Self);
        newShingle.transform.localScale = new Vector3(0.15f, lengthOfShingle, zScale);
        newShingle.transform.SetParent(buildingPart.shingleStorage.transform, false);

        return retVal;
    }

    /// <summary>
    /// Called by PlaceShinglesSimple. Places shingles along the edges of an A-frame roof.
    /// </summary>
    private static Vector3 PlaceSimpleRidgeShingles(float xShift, float yShift, float width, float side, float which, 
        Vector3 previous, BuildingPart buildingPart)
    {
        RidgeShingle newShingle = Object.Instantiate(buildingPart.ridgeShingle);
        Vector3 innerScale = buildingPart.GetScale();
        
        // Places the shingle
        newShingle.transform.position = new Vector3(
            side * ((innerScale.x / 2.0f) - xShift),
            (innerScale.y / 2.0f) + yShift,
            which * (width / 2.0f)
        );

        float lengthOfShingle = 1.2f * Vector3.Distance(previous, newShingle.transform.position);

        // Adds some beams and wooden supports under the shingles
        if (yShift < buildingPart.roofHeight && buildingPart.woodFramed) {
            DeletesIfAskedNicely newBeam = Object.Instantiate(buildingPart.simpleBeam);
            newBeam.transform.position = newShingle.transform.position;
            newBeam.transform.LookAt(previous, Vector3.up);
            newBeam.transform.position = newShingle.transform.position - new Vector3(0.0f, 0.0f, which * 0.06f);
            newBeam.transform.Translate(new Vector3(0.0f, -0.025f, 0.0f), Space.Self);
            newBeam.transform.localScale = new Vector3(0.08f, 0.05f, lengthOfShingle);
            newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);

            Beam newBeam2 = Object.Instantiate(buildingPart.beam);
            newBeam2.transform.position = newShingle.transform.position;
            newBeam2.transform.Translate(new Vector3(-side * 0.01f, -0.03f, -which * 0.375f), Space.Self);
            newBeam2.transform.localScale = new Vector3(0.7f, 0.7f, 0.8f);
            newBeam2.transform.SetParent(buildingPart.beamStorage.transform, false);
        }

        // Orients the shingle
        newShingle.transform.LookAt(previous, Vector3.up);
        Vector3 retVal = newShingle.transform.position;

        newShingle.transform.Translate(new Vector3(0.0f, 0.0f, (lengthOfShingle / 2.0f) + 0.03f));
        newShingle.transform.Rotate(-90.0f - (5.5f * buildingPart.roofCurve), 0.0f, 0.0f, Space.Self);
        newShingle.transform.Translate(new Vector3(0.0f, 0.0f, 0.01f), Space.Self);
        newShingle.transform.localScale = new Vector3(0.15f, lengthOfShingle, 0.16f);
        newShingle.transform.SetParent(buildingPart.shingleStorage.transform, false);

        return retVal;
    }

    /// <summary>
    /// Instantiates and orients a shingle.
    /// </summary>
    private static void InstantiateShingle(int index, float widthOfShingle, float lengthOfShingle, int numShinglesWide, float xShift, float yShift, float startPosition,
        float side, float angle, float tilt, bool flip, BuildingPart buildingPart)
    {
        Shingle newShingle = Object.Instantiate(buildingPart.shingle);
        Vector3 innerScale = buildingPart.GetScale();

        // In tight corners, use a mesh that has a corner cut off
        if (flip) {
            if (index == 0) newShingle.SwitchToRightCornerMesh();
            if (index == numShinglesWide - 1) newShingle.SwitchToLeftCornerMesh();
        } else {
            if (index == 0) newShingle.SwitchToLeftCornerMesh();
            if (index == numShinglesWide - 1) newShingle.SwitchToRightCornerMesh();
        }

        // Place the shingle
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

        // Rotate the shingle to align with the tangent of the roof
        if (!flip) newShingle.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
        Vector3 translateAmount = new Vector3(0.0f, -0.075f, 0.0f);
        Vector3 axis = flip ? new Vector3(1.0f, 0.0f, 0.0f) : new Vector3(0.0f, 0.0f, 1.0f);

        float zNoise = 0.025f * (Mathf.PerlinNoise(xShift / 0.7f, index / 0.8f) - 0.5f);
        float yNoise = -0.05f * (Mathf.PerlinNoise(xShift / 0.3f, index / 0.97f) - 0.5f);

        newShingle.transform.Translate(translateAmount);
        newShingle.transform.RotateAround(newShingle.transform.position - translateAmount, axis, angle);
        newShingle.transform.RotateAround(newShingle.transform.position, axis, tilt);
        newShingle.transform.localScale = new Vector3(widthOfShingle, (1.5f * lengthOfShingle) - yNoise, 0.25f);
        newShingle.transform.Translate(new Vector3(0.0f, 0.5f * yNoise, zNoise));
        newShingle.transform.SetParent(buildingPart.shingleStorage.transform, false);
    }
}
