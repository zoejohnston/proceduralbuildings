using UnityEngine;

/// <summary>
/// Some helper methods for building with beams.
/// </summary>
public static class BeamHelpers
{
    /// <summary>
    /// Places small vertical support beams on the building
    /// </summary>
    public static void PlaceVerticalBeams(int numBeamsTall, float heightOfBeam, int numBeamsWide, float widthOfBeam, float width, 
        float depth, bool flip, float side, BuildingPart buildingPart)
    {
        Vector3 innerScale = buildingPart.GetScale();

        for (int i = 0; i < numBeamsTall; i++)
        {
            for (int j = 0; j < numBeamsWide; j++)
            {
                SubBeam newBeam = Object.Instantiate(buildingPart.subBeam);

                if (flip) {
                    newBeam.transform.position = new Vector3(
                        ((j + 1) * widthOfBeam) - (width / 2.0f),
                        (i * heightOfBeam) + (heightOfBeam / 2.0f) - (innerScale.y / 2.0f),
                        side * ((depth / 2.0f) + 0.04f)
                    );
                } else {
                    newBeam.transform.position = new Vector3(
                        side * ((depth / 2.0f) + 0.04f),
                        (i * heightOfBeam) + (heightOfBeam / 2.0f) - (innerScale.y / 2.0f),
                        ((j + 1) * widthOfBeam) - (width / 2.0f)
                    );
                }

                float yRotation = flip ? side * 90.0f : (side > 0.0f ? 0.0f : 180.0f);
                newBeam.transform.Rotate(new Vector3(90.0f, 0.0f, yRotation));
                newBeam.transform.localScale = new Vector3(0.4f, 0.75f, heightOfBeam);
                newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
            }
        }

        if (flip && buildingPart.ridgeLength <= 0.2f)
        {
            SubBeam newBeam = Object.Instantiate(buildingPart.subBeam);
            newBeam.isRoofBeam = true;
            newBeam.transform.position = new Vector3(
                0.0f,
                (numBeamsTall * heightOfBeam) + (buildingPart.roofHeight / 2.0f) - (innerScale.y / 2.0f),
                side * ((depth / 2.0f) + 0.04f)
            );
            float yRotation = flip ? side * 90.0f : (side > 0.0f ? 0.0f : 180.0f);
            newBeam.transform.Rotate(new Vector3(90.0f, 0.0f, yRotation));
            newBeam.transform.localScale = new Vector3(0.4f, 0.75f, buildingPart.roofHeight);
            newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
        }
    }

    /// <summary>
    /// Places horizontal beams that split the building into multiple floors
    /// </summary>
    public static void PlaceHorizontalBeams(int numBeamsTall, float heightOfBeam, float widthOfBeam, float depth, bool flip, float side, BuildingPart buildingPart)
    {
        if (buildingPart.ridgeLength <= 0.2f && flip) numBeamsTall += 1;
        Vector3 innerScale = buildingPart.GetScale();

        int i = buildingPart.IsAboveGround() ? -1 : 0;

        while (i < numBeamsTall) {
            Beam newBeam = Object.Instantiate(buildingPart.beam);

            if (flip) {
                newBeam.transform.position = new Vector3(
                    0.0f,
                    (i * heightOfBeam) + heightOfBeam - (innerScale.y / 2.0f),
                    side * ((depth / 2.0f) + 0.04f)
                );
                newBeam.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
                newBeam.transform.localScale = new Vector3(0.5f, 1.0f, widthOfBeam);
            } else {
                newBeam.transform.position = new Vector3(
                    side * ((depth / 2.0f) + 0.04f),
                    (i * heightOfBeam) + heightOfBeam - (innerScale.y / 2.0f),
                    0.0f
                );
                newBeam.transform.localScale = new Vector3(0.5f, 1.0f, widthOfBeam);
            }
            
            newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
            i++;
        }
    }

    /// <summary>
    /// Places a large vertical beam at each corner
    /// </summary>
    public static void PlaceCornerBeams(float height, float width, float depth, BuildingPart buildingPart)
    {   
        Vector3 innerScale = buildingPart.GetScale();
        for (int i = -1; i < 2; i += 2)
        {
            for (int j = -1; j < 2; j += 2)
            {
                Beam newBeam = Object.Instantiate(buildingPart.beam);
                float positionOnRoof = 1.0f - (((depth / 2.0f) + 0.05f) / (innerScale.x / 2.0f));
                float addedHeight = Mathf.Pow(positionOnRoof, buildingPart.roofCurve) * buildingPart.roofHeight;
                float addedToBottom = 0.0f;
                if (buildingPart.ridgeLength > 0.2f) addedHeight = 0.0f;
                if (buildingPart.IsAboveGround()) addedToBottom = 0.035f;

                newBeam.transform.position = new Vector3(
                    i * ((depth / 2.0f) + 0.025f),
                    (addedHeight - addedToBottom) / 2.0f,
                    j * ((width / 2.0f) + 0.025f)
                );

                newBeam.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
                newBeam.transform.localScale = new Vector3(1.2f, 1.2f, height + addedHeight + addedToBottom);
                newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
            }
        }
    }

    /// <summary>
    /// Builds a support for a small overhang
    /// </summary>
    public static void BuildSupport(Vector3 start, Vector3 end, BuildingPart buildingPart)
    {
        Vector3 pointBelowEnd = end - new Vector3(0.0f, 0.1f, 0.0f);
        DeletesIfAskedNicely supportBeam = Object.Instantiate(buildingPart.simpleBeam);
        supportBeam.SetOtherMesh();
        supportBeam.transform.localScale = new Vector3(0.75f, 0.75f, Vector3.Distance(start, pointBelowEnd));
        supportBeam.transform.position = Vector3.Lerp(start, pointBelowEnd, 0.5f);
        supportBeam.transform.LookAt(start);
        supportBeam.transform.SetParent(buildingPart.beamStorage.transform, true);
        buildingPart.interBuildingPartObjects.Add(supportBeam.gameObject);

        pointBelowEnd -= new Vector3(0.0f, 0.05f, 0.0f);
        DeletesIfAskedNicely verticalSupportBeam = Object.Instantiate(buildingPart.simpleBeam);
        verticalSupportBeam.SetOtherMesh();
        verticalSupportBeam.transform.localScale = new Vector3(0.75f, 0.75f, Vector3.Distance(end, pointBelowEnd));
        verticalSupportBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
        verticalSupportBeam.transform.position = Vector3.Lerp(end, pointBelowEnd, 0.5f);
        verticalSupportBeam.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f), Space.Self);
        buildingPart.interBuildingPartObjects.Add(verticalSupportBeam.gameObject);
    }

    /// <summary>
    /// Builds a support for a large overhang
    /// </summary>
    public static void BuildSignificantSupport(Vector3 start, int i, int j, BuildingPart buildingPart)
    {
        RaycastHit hit;
        float supportLength = 0.0f;
        LayerMask mask = ~LayerMask.GetMask("Windows");

        if (Physics.Raycast(start, Vector3.down, out hit, start.y, mask)) {
            Beam newBeam = Object.Instantiate(buildingPart.beam);
            newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
            newBeam.transform.position = Vector3.Lerp(start, hit.point + (0.05f * Vector3.down), 0.5f);
            supportLength = Vector3.Distance(start, hit.point + (0.05f * Vector3.down));
            newBeam.transform.localScale = new Vector3(1.2f, 1.2f, supportLength);
            newBeam.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
            buildingPart.interBuildingPartObjects.Add(newBeam.gameObject);
        } else {
            Beam newBeam = Object.Instantiate(buildingPart.beam);
            Vector3 end = new Vector3(start.x, 0.0f, start.z);
            newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
            newBeam.transform.position = Vector3.Lerp(start, end, 0.5f);
            supportLength = Vector3.Distance(start, end);
            newBeam.transform.localScale = new Vector3(1.2f, 1.2f, supportLength);
            newBeam.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
            buildingPart.interBuildingPartObjects.Add(newBeam.gameObject);
        }

        DeletesIfAskedNicely supportBeam = Object.Instantiate(buildingPart.simpleBeam);
        supportBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
        supportBeam.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f), Space.Self);
        supportBeam.SetOtherMesh();

        Vector3 bottomSupportPoint = supportLength > 0.2f ? start + (0.2f * Vector3.down) : start + (supportLength * 0.9f * Vector3.down);

        Vector3 xSupportPoint = start + buildingPart.transform.TransformDirection(new Vector3(-1 * i * 0.2f, 0.0f, 0.0f));
        supportBeam.transform.position = Vector3.Lerp(bottomSupportPoint, xSupportPoint, 0.5f);
        supportBeam.transform.localScale = new Vector3(0.75f, 0.75f, Vector3.Distance(bottomSupportPoint, xSupportPoint));
        supportBeam.transform.LookAt(xSupportPoint);
        buildingPart.interBuildingPartObjects.Add(supportBeam.gameObject);

        DeletesIfAskedNicely supportBeam2 = Object.Instantiate(buildingPart.simpleBeam);
        supportBeam2.transform.SetParent(buildingPart.beamStorage.transform, false);
        supportBeam2.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f), Space.Self);
        supportBeam2.SetOtherMesh();
        
        Vector3 zSupportPoint = start + buildingPart.transform.TransformDirection(new Vector3(0.0f, 0.0f, -1 * j * 0.2f));
        supportBeam2.transform.position = Vector3.Lerp(bottomSupportPoint, zSupportPoint, 0.5f);
        supportBeam2.transform.localScale = new Vector3(0.75f, 0.75f, Vector3.Distance(bottomSupportPoint, zSupportPoint));
        supportBeam2.transform.LookAt(zSupportPoint);
        buildingPart.interBuildingPartObjects.Add(supportBeam2.gameObject);
    }
}
