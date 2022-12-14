using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCamera : MonoBehaviour
{
    [Header("Camera pan speed")]
    public float speed;

    [Header("Limits for paning")]
    public float xBorderMin;
    public float xBorderMax;
    public float yBorderMin;
    public float yBorderMax;

    public bool IsMoved;
    void Update()
    {

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            IsMoved = true;
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            Debug.Log($"X:{touchDeltaPosition.x}; Y:{touchDeltaPosition.y}");
            transform.Translate(-touchDeltaPosition.x * speed * Time.deltaTime, -touchDeltaPosition.y * speed * Time.deltaTime, 0);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, xBorderMin, xBorderMax), Mathf.Clamp(transform.position.y, yBorderMin, yBorderMax), 0);
        }
        else { IsMoved = false; }
    }
}
