using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Vuforia;

public class FlashLight : MonoBehaviour {
    
    bool isFlashLightOn = false;

    public void ToggleFlash()
    {
        if (isFlashLightOn)
        {
            Debug.Log("Light is on... turning off");
            CameraDevice.Instance.SetFlashTorchMode(false);
            isFlashLightOn = false;
        }
        else
        {
            Debug.Log("Light is off... turning on");
            CameraDevice.Instance.SetFlashTorchMode(true);
            isFlashLightOn = true;
        }
    }    
}
