using UnityEngine;

/// <summary>
/// Some helper methods for building with beams.
/// </summary>
public static class BeamHelpers
{
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

    public static void PlaceHorizontalBeams(int numBeamsTall, float heightOfBeam, float widthOfBeam, float depth, bool flip, float side, BuildingPart buildingPart)
    {
        if (buildingPart.ridgeLength <= 0.2f && flip) numBeamsTall += 1;
        Vector3 innerScale = buildingPart.GetScale();

        for (int i = 0; i < numBeamsTall; i++)
        {
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
        }
    }

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
                if (buildingPart.ridgeLength > 0.2f) addedHeight = 0.0f;

                newBeam.transform.position = new Vector3(
                    i * ((depth / 2.0f) + 0.025f),
                    addedHeight / 2.0f,
                    j * ((width / 2.0f) + 0.025f)
                );

                newBeam.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
                newBeam.transform.localScale = new Vector3(1.2f, 1.2f, height + addedHeight);
                newBeam.transform.SetParent(buildingPart.beamStorage.transform, false);
            }
        }
    }
}
