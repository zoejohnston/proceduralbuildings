using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BrickCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private Brick CopyBrick(GameObject parentObject)
    {
        GameObject newBrickObject = Instantiate(parentObject);
        Brick newBrick = newBrickObject.GetComponent<Brick>();
        newBrickObject.transform.SetParent(parentObject.transform.parent);

        return newBrick;
    }

    /*
    */
    private void HandleBrickCollisions(GameObject parentObject, Brick brick, float horizontalMin, float horizontalMax, float verticalMin, float verticalMax)
    {
        // If the brick is entirely within the vertical bounds of the collider
        if (-0.5f > verticalMin && 0.5f < verticalMax) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) { 
                brick.DeletePls(); 

            } else if (horizontalMax > 0.5f) {
                brick.ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f) {
                brick.ResizeRight(horizontalMax);

            } else {
                Brick newBrickRight = CopyBrick(parentObject);
                newBrickRight.ResizeLeft(horizontalMin);
                brick.ResizeRight(horizontalMax);
            }
        // If the top of the brick is within the vertical bounds of the collider
        } else if (-0.5f > verticalMin) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) { 

            } else if (horizontalMax > 0.5f) {
                Brick newBrick = CopyBrick(parentObject);
                newBrick.ResizeTop(verticalMax);
                newBrick.ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f) {
                Brick newBrick = CopyBrick(parentObject);
                newBrick.ResizeTop(verticalMax);
                newBrick.ResizeRight(horizontalMax);

            } else {
                Brick newBrickLeft = CopyBrick(parentObject);
                newBrickLeft.ResizeTop(verticalMax);
                newBrickLeft.ResizeRight(horizontalMax);
                Brick newBrickRight = CopyBrick(parentObject);
                newBrickRight.ResizeTop(verticalMax);
                newBrickRight.ResizeLeft(horizontalMin);
            }

            brick.ResizeBottom(verticalMax); 
        // If the bottom of the brick is within the vertical bounds of the collider
        } else if (0.5f < verticalMax) {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) {  

            } else if (horizontalMax > 0.5f) {
                Brick newBrick = CopyBrick(parentObject);
                newBrick.ResizeBottom(verticalMin);
                newBrick.ResizeLeft(horizontalMin);

            } else if (horizontalMin < -0.5f) {
                Brick newBrick = CopyBrick(parentObject);
                newBrick.ResizeBottom(verticalMin);
                newBrick.ResizeRight(horizontalMax);

            } else {
                Brick newBrickLeft = CopyBrick(parentObject);
                newBrickLeft.ResizeBottom(verticalMin);
                newBrickLeft.ResizeRight(horizontalMax);
                Brick newBrickRight = CopyBrick(parentObject);
                newBrickRight.ResizeBottom(verticalMin);
                newBrickRight.ResizeLeft(horizontalMin);
            }

            brick.ResizeTop(verticalMin);
        // If the collider is entirely within the vertical bounds of the brick
        } else {
            if (horizontalMax > 0.5f && horizontalMin < -0.5f) {  
                Brick newBrick = CopyBrick(parentObject);
                newBrick.ResizeBottom(verticalMax);
                brick.ResizeTop(verticalMin);

            } else if (horizontalMax > 0.5f) {
                Brick newBrickTop = CopyBrick(parentObject);
                newBrickTop.ResizeBottom(verticalMax);
                Brick newBrickRight = CopyBrick(parentObject);
                newBrickRight.ResizeLeft(horizontalMin);
                newBrickRight.ResizeTop(verticalMax);
                brick.ResizeTop(verticalMin);
                brick.ResizeRight(horizontalMin);

            } else if (horizontalMin < -0.5f) {
                Brick newBrickTop = CopyBrick(parentObject);
                newBrickTop.ResizeBottom(verticalMax);
                Brick newBrickLeft = CopyBrick(parentObject);
                newBrickLeft.ResizeRight(horizontalMax);
                newBrickLeft.ResizeTop(verticalMax);
                brick.ResizeTop(verticalMin);
                brick.ResizeLeft(horizontalMax);

            } else {
                Brick newBrickTop = CopyBrick(parentObject);
                newBrickTop.ResizeBottom(verticalMax);
                newBrickTop.ResizeRight(horizontalMin);
                Brick newBrickRight = CopyBrick(parentObject);
                newBrickRight.ResizeTop(verticalMax);
                newBrickRight.ResizeRight(horizontalMax);
                Brick newBrickLeft = CopyBrick(parentObject);
                newBrickLeft.ResizeBottom(verticalMin);
                newBrickLeft.ResizeLeft(horizontalMin);
                brick.ResizeTop(verticalMin);
                brick.ResizeLeft(horizontalMax);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject parentObject = gameObject.transform.parent.gameObject;
        Brick brick = parentObject.GetComponent<Brick>();

        if (other.gameObject.transform.parent.gameObject.GetComponent<Quoin>()) {
            Quoin quoin = other.gameObject.transform.parent.gameObject.GetComponent<Quoin>();

            Vector3 horizontalDirection = brick.transform.TransformDirection(Vector3.forward);
            Vector3 quoinSpaceHorizontalDirection = quoin.transform.InverseTransformDirection(horizontalDirection);
            float horizontalScale = Vector3.Dot(quoinSpaceHorizontalDirection, quoin.transform.localScale) / 2.0f;
            Vector3 worldSpaceHorizontalDirection = horizontalScale * horizontalDirection;
            float horizontalMin = transform.InverseTransformPoint(quoin.transform.position - worldSpaceHorizontalDirection).z;
            float horizontalMax = transform.InverseTransformPoint(quoin.transform.position + worldSpaceHorizontalDirection).z;
            //Debug.DrawLine(quoin.transform.position - worldSpaceHorizontalDirection, quoin.transform.position + worldSpaceHorizontalDirection, Color.blue);
            
            Vector3 verticalDirection = brick.transform.TransformDirection(Vector3.up);
            float verticalScale = Vector3.Dot(verticalDirection, quoin.transform.localScale) / 2.0f;
            Vector3 worldSpaceVerticalDirection = verticalScale * verticalDirection;
            float verticalMin = transform.InverseTransformPoint(quoin.transform.position - worldSpaceVerticalDirection).y;
            float verticalMax = transform.InverseTransformPoint(quoin.transform.position + worldSpaceVerticalDirection).y;
           // Debug.DrawLine(quoin.transform.position - worldSpaceVerticalDirection, quoin.transform.position + worldSpaceVerticalDirection, Color.red);

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            HandleBrickCollisions(parentObject, brick, horizontalMin, horizontalMax, verticalMin, verticalMax);

        } else if (other.gameObject.transform.parent.gameObject.GetComponent<Window>()) {
            Window window = other.gameObject.transform.parent.gameObject.GetComponent<Window>();

            Vector3 horizontalDirection = new Vector3(0.0f, 0.0f, window.transform.GetChild(0).localScale.z / 2.0f);
            Vector3 worldSpaceHorizontalDirection = window.transform.TransformDirection(horizontalDirection);
            float horizontalMin = transform.InverseTransformPoint(window.transform.position - worldSpaceHorizontalDirection).z;
            float horizontalMax = transform.InverseTransformPoint(window.transform.position + worldSpaceHorizontalDirection).z;

            Vector3 verticalDirection = new Vector3(0.0f, window.transform.GetChild(0).localScale.y / 2.0f, 0.0f);
            Vector3 worldSpaceVerticalDirection = window.transform.TransformDirection(verticalDirection);
            float verticalMin = transform.InverseTransformPoint(window.transform.position - worldSpaceVerticalDirection).y;
            float verticalMax = transform.InverseTransformPoint(window.transform.position + worldSpaceVerticalDirection).y;

            if (horizontalMax < horizontalMin) {
                float temp = horizontalMin;
                horizontalMin = horizontalMax;
                horizontalMax = temp;
            }

            HandleBrickCollisions(parentObject, brick, horizontalMin, horizontalMax, verticalMin, verticalMax);
        }
    }
}
