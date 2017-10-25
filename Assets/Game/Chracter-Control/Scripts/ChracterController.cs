using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChracterController : MonoBehaviour {

    public float forceMult = 30.0f;
    public float forceMultDirection = 60.0f;

    private Rigidbody rb;

    void Start() 
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() 
    {
        {
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
                float moveH = Input.GetAxis("Horizontal");
                float moveV = Input.GetAxis("Vertical");

                Vector3 move = new Vector3(moveH * forceMultDirection, 0.0f, moveV * forceMult);
                move = Camera.main.transform.TransformDirection(move);
                rb.AddForce(move);
            }
            else
            {
                // TODO: Test
                float moveH = Input.acceleration.x;
                float moveV = -Input.acceleration.z;

                Vector3 move = new Vector3(moveH * forceMultDirection, 0.0f, moveV * forceMult);
                rb.AddForce(move);
            }
        }
    }
}
