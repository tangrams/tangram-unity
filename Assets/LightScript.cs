using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightScript : MonoBehaviour
{
    float angle = 0.0f;

    // Use this for initialization
    void Start()
    {
		
    }
	
    // Update is called once per frame
    void Update()
    {
        this.angle = Time.time * 0.01f;
        this.transform.RotateAround(
            new Vector3(0.0f, 0.0f, 0.0f), 
            new Vector3(0.0f, 1.0f, 0.0f), 
            angle);
        this.transform.RotateAround(
            new Vector3(0.0f, 0.0f, 0.0f), 
            new Vector3(0.0f, 0.0f, 1.0f), 
            angle * 0.5f);
    }
}
