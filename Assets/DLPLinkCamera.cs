using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DLPLinkCamera : MonoBehaviour
{
    public float BlankFrameRate = 50;
    public int CameraFrameModulo;
    public UnityEvent<string> OnReportFrameRate;

    Camera cam;
    Color cachedColor;
    int frameCount;
    float prevTime;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        cachedColor = cam.backgroundColor;
    }

    private void DoFrame()
    {
        frameCount++;

        var frame = frameCount;//Time.renderedFrameCount;

        OnReportFrameRate?.Invoke((1f / (Time.time - prevTime)).ToString("F2"));
        prevTime = Time.time;

        //if (frame % 2 == 0)
        {
        //    cam.backgroundColor = Color.white;
        }
        //else
        if(frame % 2 == CameraFrameModulo)
        {
            cam.backgroundColor = cachedColor;
        }
        else
        {
            cam.backgroundColor = Color.black;
        }
    }

    public void SetBlankFrameRate(float value)
    {
        BlankFrameRate = value;
        CancelInvoke(nameof(DoFrame));
        InvokeRepeating(nameof(DoFrame), 0f, 1f / BlankFrameRate);
    }

    public void SetBlankFrameRate(string value)
    {
        SetBlankFrameRate(float.Parse(value));
    }
}
