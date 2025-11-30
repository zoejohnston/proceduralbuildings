using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MainBuildingPartCollider : MonoBehaviour
{
    public bool PointIsInside(Vector3 point)
    {
        BoxCollider mainSection = gameObject.GetComponent<BoxCollider>();
        Vector3 closest = mainSection.ClosestPoint(point);
        //if (closest == point) return true;
        if ((closest - point).sqrMagnitude < 0.01f * 0.01f) return true;

        return false;
    }

    public bool AllPointsAreInside(Transform otherTransform, float x, float y, float z)
    {
        for (int i = -1; i < 2; i += 2) {
            for (int j = -1; j < 2; j += 2) {
                for (int k = -1; k < 2; k += 2) {
                    Vector3 point = otherTransform.TransformPoint(new Vector3(x * i, y * j, z * k));
                    if (!PointIsInside(point)) return false;
                }
            }
        }

        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.transform.root != gameObject.transform.root) {
            if (other.gameObject.transform.parent.gameObject.TryGetComponent<Brick>(out Brick brick)) {
                if (AllPointsAreInside(other.gameObject.transform, 0.5f, 0.5f, 0.5f)) {
                    brick.DeletePls();
                }
            } else if (other.gameObject.transform.parent.gameObject.TryGetComponent<Quoin>(out Quoin quoin)) {
                if (AllPointsAreInside(other.gameObject.transform, 0.5f, 0.5f, 0.5f)) {
                    quoin.DeletePls();
                }
            } else if (other.gameObject.transform.parent.gameObject.TryGetComponent<Shingle>(out Shingle shingle)) {
                if (AllPointsAreInside(other.gameObject.transform, 0.5f, 0.5f, 0.5f)) {
                    shingle.DeletePls();
                }
            } else if (other.gameObject.transform.parent.gameObject.TryGetComponent<Beam>(out Beam beam)) {
                if (AllPointsAreInside(other.gameObject.transform, 0.025f, 0.025f, 0.5f)) {
                    beam.DeletePls();
                }
            } else if (other.gameObject.transform.parent.gameObject.TryGetComponent<SubBeam>(out SubBeam subBeam)) {
                if (AllPointsAreInside(other.gameObject.transform, 0.025f, 0.025f, 0.5f)) {
                    subBeam.DeletePls();
                }
            } else if (other.gameObject.transform.parent.gameObject.TryGetComponent<RidgeShingle>(out RidgeShingle ridgeShingle)) {
                if (AllPointsAreInside(other.gameObject.transform, 0.225f, 0.5f, 0.15f)) {
                    ridgeShingle.DeletePls();
                }
            }
        }*/
    }
}
