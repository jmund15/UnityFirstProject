using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 20.0f;

    public Vector3 cameraRotate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float horizInput = Input.GetAxis("HorizCamera");
        float vertInput = Input.GetAxis("VertCamera");
        cameraRotate = new Vector3(0, horizInput, 0);
        transform.Rotate(cameraRotate, -Mathf.Abs(horizInput) * rotationSpeed * Time.deltaTime); // -Math.Abs is for rotating correct direction correllating with key press.

    }
}
