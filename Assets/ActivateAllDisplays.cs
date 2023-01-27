using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using Apt.Unity.Projection;
using UnityEngine.UI;

public class ActivateAllDisplays : MonoBehaviour
{
    public Camera targetCam;
    public StereoscopicCamera stereoCam;
    public LayerMask Near;
    public LayerMask Far;
    public LayerMask Nothing;
    public LayerMask Everything;
    public float NearDistance;
    public float FarDistance;
    public ProjectionPlane projectionPlane;
    public Slider depthSlider;

    void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
        // Check if additional displays are available and activate each.

        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    void Update()
    {
        
    }

    public void ToggleStereoscopy()
    {
        if(targetCam.stereoTargetEye == StereoTargetEyeMask.None)
            targetCam.stereoTargetEye = StereoTargetEyeMask.Both;
        else
            targetCam.stereoTargetEye = StereoTargetEyeMask.None;
    }

    public void SetAccommodationPlane(int index)
    {
        switch (index)
        {
            case 0:
                targetCam.cullingMask = Far;
                stereoCam.LeftEye.cullingMask = Near;
                stereoCam.RightEye.cullingMask = Near;
                projectionPlane.transform.position = Vector3.forward * NearDistance;
                projectionPlane.Size.x = 1f;
                depthSlider.maxValue = FarDistance;
                depthSlider.minValue = -NearDistance;
                depthSlider.value = 0f;
                break;
            case 1:
                targetCam.cullingMask = Nothing;
                stereoCam.LeftEye.cullingMask = Everything;
                stereoCam.RightEye.cullingMask = Everything;
                projectionPlane.transform.position = Vector3.forward * FarDistance;
                projectionPlane.Size.x = 3f;
                depthSlider.maxValue = 0f;
                depthSlider.minValue = -FarDistance;
                depthSlider.value = 0f;
                break;
        }
    }
}
