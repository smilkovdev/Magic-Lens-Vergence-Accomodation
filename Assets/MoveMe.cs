using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveMe : MonoBehaviour
{
    public float Speed = 1f;
    public Vector2 ClampPositionZ;
    public UnityEvent<float> OnKeyPositionZChange;

    Vector3 startSize;

    // Start is called before the first frame update
    void Start()
    {
        startSize = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        /*var changed = false;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
            changed = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(-Vector3.forward * Speed * Time.deltaTime);
            changed = true;
        }

        if (changed)
        {
            transform.position = Vector3.forward * Mathf.Clamp(transform.position.z, ClampPositionZ.x, ClampPositionZ.y);
            OnKeyPositionZChange?.Invoke(transform.position.z);
        }*/

        var size = transform.position.z;//(/*Camera.main.transform.position*/ - transform.position).magnitude;
        transform.localScale = new Vector3(size * startSize.x, size * startSize.y, size * startSize.z);
    }

    public void SetPositionZ(float z)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    }
}
