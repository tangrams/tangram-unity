using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChracterController : MonoBehaviour {

    public float forceMult = 30.0f;
    public float maxVelocity = 50.0f;

    private Rigidbody rb;
    private float maxSqVelocity;

    void Awake() // Recommended to use Awake instead of Start here.
    {
        rb = GetComponent<Rigidbody>();
        maxSqVelocity = maxVelocity * maxVelocity;

    }

    void FixedUpdate() 
    {
        if (rb.velocity.sqrMagnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
        {
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
                float moveH = Input.GetAxis("Horizontal");
                float moveV = Input.GetAxis("Vertical");

                Vector3 move = new Vector3(moveH, 0.0f, moveV);
                move = Camera.main.transform.TransformDirection(move);
                move.y = 0.0f;
                rb.AddForce(move * forceMult);
            }
            else
            {
                // TODO: Test
                float moveH = Input.acceleration.x;
                float moveV = -Input.acceleration.z;

                Vector3 move = new Vector3(moveH, 0.0f, moveV);
                move = Camera.main.transform.TransformDirection(move);
                move.y = 0.0f;
                rb.AddForce(move * maxVelocity);
            }
        }
    }
}
