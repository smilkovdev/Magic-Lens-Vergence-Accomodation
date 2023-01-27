using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static EyeTestSurface;

[RequireComponent(typeof(Camera))]
public class StereoscopicCamera : MonoBehaviour
{
    public Camera OtherEye;
    public float Speed;
    public UnityEvent<string> OnReportDistance;
    public Transform Obj;

    Camera MyEye;
    Vector3 prevMousePosition;
    bool dragging;

    public float StereoDisparity => Vector3.Distance(MyEye.transform.position, OtherEye.transform.position);
    public Camera LeftEye => MyEye;
    public Camera RightEye => OtherEye;

    public float EyeHorizontal
    {
        get { return MyEye.transform.position.x; }
        set
        {
            MyEye.transform.position = new Vector3(
                value,
                MyEye.transform.position.y,
                MyEye.transform.position.z);
            OtherEye.transform.position = new Vector3(
                -value,
                OtherEye.transform.position.y,
                OtherEye.transform.position.z);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MyEye = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Display.RelativeMouseAt(Input.mousePosition).z != MyEye.targetDisplay)
            return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            prevMousePosition = Input.mousePosition;
            dragging = true;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (Input.GetMouseButton(0) && dragging)
        {
            //Debug.Log($"Dragging on display {cam.targetDisplay} with camera {cam.gameObject.name}");
            var delta = Input.mousePosition - prevMousePosition;
            
            MyEye.transform.Translate(-delta * Speed);
            OtherEye.transform.Translate(-delta * Speed);
            //Obj.Translate(-delta * Speed);
            foreach(Transform child in Obj.parent)
            {
                child.Translate(-delta * Speed);
            }

            prevMousePosition = Input.mousePosition;
        }

        if (Input.mouseScrollDelta.sqrMagnitude > 0f)
        {
            var dist = Input.mouseScrollDelta.y * Speed * .5f;
            var eyeSurfaces = FindObjectsOfType<EyeTestSurface>();
            foreach(var eyeSurface in eyeSurfaces)
            {
                if (eyeSurface.surfaceType == SurfaceType.NEAR)
                    continue;

                eyeSurface.transform.Translate(new Vector3(eyeSurface.order == 0 ? -dist : dist, 0, 0));
            }
            /*MyEye.transform.Translate(Vector3.right * dist);
            OtherEye.transform.Translate(Vector3.left * dist);

            // Distance calculation
            // D = B / (tan a1 + tan a2)
            // B = distance between stereo cameras
            // a1 = angle between feature point and cam 1 forward
            // a1 = angle between feature point and cam 2 forward
            var B = Vector3.Distance(MyEye.transform.position, OtherEye.transform.position);
            var fpWorld = new Vector3(0, 0, 1);
            var fp1 = MyEye.transform.InverseTransformPoint(fpWorld);
            var fp2 = OtherEye.transform.InverseTransformPoint(fpWorld);
            var a1 = Vector3.Angle(fp1, MyEye.transform.forward) * Mathf.Deg2Rad;
            var a2 = Vector3.Angle(fp2, OtherEye.transform.forward) * Mathf.Deg2Rad;
            var D = B / (Mathf.Tan(a1) + Mathf.Tan(a2));

            //OnReportDistance?.Invoke(D.ToString());

            // Distance calculation
            // The two camera positions are stereo points on the projection plane,
            // they are the even angles on an isosceles triangle.
            // h = sqrt(b^2-1/4a^2)
            // b = distance from cam to feature point
            // a = distance between cams
            var b = fp1.magnitude;
            var a = Vector3.Distance(MyEye.transform.position, OtherEye.transform.position);
            var h = Mathf.Sqrt((b * b) - (1/4f * (a * a)));
            //OnReportDistance?.Invoke(h.ToString());

            // Parallax angle
            // theta = 2 * atan(DX / 2*d)
            // DX = horizontal parallax distance (distance between cams)
            // d = distance from projection plane to eyes
            var DX = MyEye.transform.position.x * 2;
            var d = 0.6f;
            var theta = 2f * Mathf.Atan(DX / 2 * d);
            OnReportDistance?.Invoke(theta.ToString());*/
        }
    }

}
