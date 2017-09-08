using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlinkScript : MonoBehaviour
{
    public Light light;
    public float defaultIntensity;

    void Start()
    {
        light = GetComponent<Light>();
        defaultIntensity = light.intensity;
    }
	
    // Update is called once per frame
    void Update()
    {
        light.intensity = (Mathf.Cos(Time.time) * 0.5f + 0.5f) * defaultIntensity * 2.0f;
    }
}
