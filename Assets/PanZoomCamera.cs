using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PanZoomCamera : MonoBehaviour
{
    public float Speed;
    public float ZoomSpeed;
    //public Transform ZoomObject;

    Camera cam;
    Vector3 prevMousePosition;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Display.RelativeMouseAt(Input.mousePosition).z != cam.targetDisplay)
            return;

        if (Input.GetMouseButtonDown(0))
            prevMousePosition = Input.mousePosition;

        if(Input.GetMouseButton(0))
        {
            var delta = Input.mousePosition - prevMousePosition;
            cam.transform.Translate(delta * Speed);
            prevMousePosition = Input.mousePosition;
        }

        if(Input.mouseScrollDelta.sqrMagnitude > 0f)
        {
            //ZoomObject.localScale = ZoomObject.localScale + Vector3.one * Input.mouseScrollDelta.y * ZoomSpeed;
            cam.transform.Translate(Vector3.forward * Input.mouseScrollDelta.y * ZoomSpeed);
        }
    }
}
