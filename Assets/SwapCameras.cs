using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SwapCameras : MonoBehaviour
{
    public Camera OtherCamera;

    public void Swap()
    {
        var myCamera = GetComponent<Camera>();
        var cachedDisplay = myCamera.targetDisplay;
        var cachedEye = myCamera.stereoTargetEye;

        myCamera.targetDisplay = OtherCamera.targetDisplay;
        myCamera.stereoTargetEye = OtherCamera.stereoTargetEye;
        OtherCamera.targetDisplay = cachedDisplay;
        OtherCamera.stereoTargetEye = cachedEye;

        var myPanZoom = GetComponent<PanZoomCamera>();
        var otherPanZoom = OtherCamera.GetComponent<PanZoomCamera>();
        //var cachedObj = myPanZoom.ZoomObject;

        //myPanZoom.ZoomObject = otherPanZoom.ZoomObject;
        //otherPanZoom.ZoomObject = cachedObj;
    }
}
