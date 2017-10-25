using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChracterController : MonoBehaviour {

    public float speed = 1.0f;
    public float forceMult = 1.0f;
    public float turnSpeed = 1.0f;

    private Rigidbody rb;
    private float powerInput;
    private float turnInput;

    void Start() 
    {
        rb = GetComponent<Rigidbody>();
    }

    void update() 
    {
        powerInput = Input.GetAxis ("Vertical");
        turnInput = Input.GetAxis ("Horizontal");
    }

    void FixedUpdate() 
    {
        
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            float moveH = Input.GetAxis("Horizontal");
            float moveV = Input.GetAxis("Vertical");

            Vector3 move = new Vector3(moveH, 0.0f, moveV);
            rb.AddForce(move * forceMult);
        }
        else
        {
            // TODO: Test
            float moveH = Input.acceleration.x;
            float moveV = -Input.acceleration.z;

            Vector3 move = new Vector3(moveH, 0.0f, moveV);
            rb.AddForce(move * forceMult);
        }

        rb.AddRelativeForce(0f, 0f, powerInput * speed);
        rb.AddRelativeTorque(0f, turnInput * turnSpeed, 0f);

    }
}
