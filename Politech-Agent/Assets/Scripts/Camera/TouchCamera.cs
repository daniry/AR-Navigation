// Just add this script to your camera. It doesn't need any configuration.

using UnityEngine;

public class TouchCamera : MonoBehaviour
{

    public ScrollDDFix fix;

    Vector2?[] oldTouchPositions = {
        null,
        null
    };
    Vector2 oldTouchVector;
    float oldTouchDistance;
    public bool IsMoved;

    private float ortoSize;
    private Vector3 pos;

    void Update()
    {
        if (!fix.isOver)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                IsMoved = true;
            }
            else
            {
                IsMoved = false;
            }

            if (Input.touchCount == 0)
            {
                oldTouchPositions[0] = null;
                oldTouchPositions[1] = null;
            }
            else if (Input.touchCount == 1)
            {
                if (oldTouchPositions[0] == null || oldTouchPositions[1] != null)
                {
                    oldTouchPositions[0] = Input.GetTouch(0).position;
                    oldTouchPositions[1] = null;
                }
                else
                {
                    Vector2 newTouchPosition = Input.GetTouch(0).position;

                    pos = transform.position + transform.TransformDirection((Vector3)((oldTouchPositions[0] - newTouchPosition) * GetComponent<Camera>().orthographicSize / GetComponent<Camera>().pixelHeight * 2f));
                    pos = new Vector3(Mathf.Clamp(pos.x, -25, 30), Mathf.Clamp(pos.y, -7, 3), Mathf.Clamp(pos.z, -21, 24));

                    transform.position = pos;

                    oldTouchPositions[0] = newTouchPosition;
                }
            }
            else
            {
                if (oldTouchPositions[1] == null)
                {
                    oldTouchPositions[0] = Input.GetTouch(0).position;
                    oldTouchPositions[1] = Input.GetTouch(1).position;
                    oldTouchVector = (Vector2)(oldTouchPositions[0] - oldTouchPositions[1]);
                    oldTouchDistance = oldTouchVector.magnitude;
                }
                else
                {
                    Vector2 screen = new Vector2(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight);

                    Vector2[] newTouchPositions = {
                    Input.GetTouch(0).position,
                    Input.GetTouch(1).position
                };
                    Vector2 newTouchVector = newTouchPositions[0] - newTouchPositions[1];
                    float newTouchDistance = newTouchVector.magnitude;

                    pos = transform.position + transform.TransformDirection((Vector3)((oldTouchPositions[0] + oldTouchPositions[1] - screen) * GetComponent<Camera>().orthographicSize / screen.y));
                    pos = new Vector3(Mathf.Clamp(pos.x, -25, 30), Mathf.Clamp(pos.y, -7, 3), Mathf.Clamp(pos.z, -21, 24));
                    transform.position = pos;
                    //transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, Mathf.Asin(Mathf.Clamp((oldTouchVector.y * newTouchVector.x - oldTouchVector.x * newTouchVector.y) / oldTouchDistance / newTouchDistance, -1f, 1f)) / 0.0174532924f));

                    ortoSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize * oldTouchDistance / newTouchDistance, 5, 60);
                    GetComponent<Camera>().orthographicSize = ortoSize;
                    pos = transform.position - transform.TransformDirection((newTouchPositions[0] + newTouchPositions[1] - screen) * GetComponent<Camera>().orthographicSize / screen.y);
                    pos = new Vector3(Mathf.Clamp(pos.x, -25, 30), Mathf.Clamp(pos.y, -7, 3), Mathf.Clamp(pos.z, -21, 24));
                    transform.position = pos;
                    oldTouchPositions[0] = newTouchPositions[0];
                    oldTouchPositions[1] = newTouchPositions[1];
                    oldTouchVector = newTouchVector;
                    oldTouchDistance = newTouchDistance;
                }
            }
        }
   }
}
