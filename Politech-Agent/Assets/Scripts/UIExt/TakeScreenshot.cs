using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{
    int i = 0;
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            ScreenCapture.CaptureScreenshot($"Screenshot_{i}.png");
            i++;
        }
    }
}
